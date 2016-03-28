using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OneDrive.Sdk;

namespace OneDriveWebhookTranslator.Models
{
    class OneDriveAccountServiceProvider : Microsoft.OneDrive.Sdk.IServiceInfoProvider
    {
        private readonly OneDriveUser user;
        private readonly DelegateAuthenticationProvider authProvider;
        private readonly string resource;

        public OneDriveAccountServiceProvider(OneDriveUser user)
        {
            this.user = user;

            Uri baseUrl;
            if (!Uri.TryCreate(user.OneDriveBaseUrl, UriKind.Absolute, out baseUrl))
            {
                throw new InvalidOperationException("Unable to parse base URL: " + user.OneDriveBaseUrl);
            }

            this.resource = string.Concat(baseUrl.Scheme, "://", baseUrl.Host);
            this.authProvider = new DelegateAuthenticationProvider(new DelegateAuthenticationProvider.ProviderAuthHeaderDelegate(async req =>
            {
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await this.user.GetAccessTokenAsync(this.resource));
            }));
        }

        public IAuthenticationProvider AuthenticationProvider
        {
            get { return this.authProvider; }
        }

        public Task<ServiceInfo> GetServiceInfo(AppConfig appConfig, CredentialCache credentialCache, IHttpProvider httpProvider, ClientType clientType)
        {
            var info = new ServiceInfo
            {
                AccountType = AccountType.None,
                AuthenticationProvider = this.authProvider,
                CredentialCache = credentialCache ?? new CredentialCache(),
                HttpProvider = httpProvider ?? new HttpProvider(),
                BaseUrl = this.user.OneDriveBaseUrl
            };
            return Task.FromResult(info);
        }
    }

    class DelegateAuthenticationProvider : IAuthenticationProvider
    {
        public delegate Task ProviderAuthHeaderDelegate(HttpRequestMessage request);

        private readonly ProviderAuthHeaderDelegate methodDelegate;

        public DelegateAuthenticationProvider(ProviderAuthHeaderDelegate method)
        {
            this.methodDelegate = method;
            this.CurrentAccountSession = new AccountSession();
        }
            

        public AccountSession CurrentAccountSession { get; set; }

        public async Task AppendAuthHeaderAsync(HttpRequestMessage request)
        {
            await this.methodDelegate(request);
        }

        public Task<AccountSession> AuthenticateAsync()
        {
            return Task.FromResult(this.CurrentAccountSession);
        }

        public Task SignOutAsync()
        {
            return Task.FromResult(false);
        }
    }
}
