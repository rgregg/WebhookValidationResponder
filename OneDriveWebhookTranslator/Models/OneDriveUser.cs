using OneDriveWebhookTranslator.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveWebhookTranslator.Models
{
    public class OneDriveUser
    {
        private Dictionary<string, OAuthToken> TokenCache = new Dictionary<string, OAuthToken>();
        private readonly string UserId;
        private string RefreshToken { get; set; }
        private OAuthHelper AuthHelper { get; set; }

        public string OneDriveBaseUrl { get; set; }

        public string SubscriptionId { get; set; }

        public string DeltaToken { get; set; }

        public Microsoft.OneDrive.Sdk.ClientType ClientType { get; set; }

        public Dictionary<string, string> FileNameAndETag { get; set; }

        public OneDriveUser(OAuthToken token, OAuthHelper helper, string resource = null)
        {
            this.UserId = Guid.NewGuid().ToString();
            this.AuthHelper = helper;
            this.ClientType = helper.IsConsumerService ? Microsoft.OneDrive.Sdk.ClientType.Consumer : Microsoft.OneDrive.Sdk.ClientType.Business;
            this.FileNameAndETag = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(token.RefreshToken))
            {
                this.RefreshToken = token.RefreshToken;
            }

            if (!string.IsNullOrEmpty(resource))
            {
                TokenCache[resource] = token;
            }


            OneDriveUserManager.RegisterUser(this.UserId, this);
        }

        public async Task<string> GetAccessTokenAsync(string resource)
        {
            // Return a cached access token if we still have a valid one for this resource
            OAuthToken token;
            if (TokenCache.TryGetValue(resource, out token))
            {
                if (!string.IsNullOrEmpty(token.AccessToken) &&
                    token.CreatedDateTime.AddSeconds(token.AccessTokenExpirationDuration) > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    return token.AccessToken;
                }
            }

            // Otherwise, we need to redeem the refresh token
            token = await this.AuthHelper.RedeemRefreshTokenAsync(this.RefreshToken, resource);
            TokenCache[resource] = token;

            return token.AccessToken;
        }

        internal void SetResponseCookie(HttpResponseBase response)
        {
            HttpCookie cookie = new HttpCookie("oneDriveUser", this.UserId);
            cookie.HttpOnly = true;

            response.SetCookie(cookie);
        }

        internal static OneDriveUser UserForRequest(HttpRequestBase request)
        {
            try
            {
                string userGuid = request.Cookies["oneDriveUser"].Value;
                return OneDriveUserManager.LookupUserById(userGuid);
            }
            catch { }

            return null;
        }

        internal static void ClearResponseCookie(HttpResponseBase response)
        {
            HttpCookie cookie = new HttpCookie("oneDriveUser", null);
            cookie.HttpOnly = true;
            response.SetCookie(cookie);
        }
    }
}
