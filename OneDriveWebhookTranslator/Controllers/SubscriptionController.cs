using OneDriveWebhookTranslator.Auth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.OneDrive.Sdk;
using OneDriveWebhookTranslator.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace OneDriveWebhookTranslator.Controllers
{
    public class SubscriptionController : Controller
    {
        // GET: Subscription
        public ActionResult Index()
        {
            return View();
        }

        // Create webhook subscription
        public async Task<ActionResult> CreateSubscription()
        {
            #region Create OneDriveClient for current user
            OneDriveUser user = OneDriveUser.UserForRequest(this.Request);
            if (null == user)
            {
                return Redirect(Url.Action("Index", "Home"));
            }
            var client = await GetOneDriveClientAsync(user);
            #endregion

            // Ensure the app folder is created first
            var appFolder = await client.Drive.Special["approot"].Request().GetAsync();

            // Create a subscription on the drive
            var notificationUrl = ConfigurationManager.AppSettings["ida:NotificationUrl"];

            Models.OneDriveSubscription subscription = new OneDriveSubscription
            {
                NotificationUrl = notificationUrl,
                ClientState = "my client state"
            };
            FixPPESubscriptionBug(user, subscription);

            // Because the OneDrive SDK does not support OneDrive subscriptions natively yet, 
            // we use BaseRequest to generate a request the SDK can understand. You could also use HttpClient
            var request = new BaseRequest(client.BaseUrl + "/drive/root/subscriptions", client)
            {
                Method = "POST",
                ContentType = "application/json"
            };

            try
            {
                var subscriptionResponse = await request.SendAsync<Models.OneDriveSubscription>(subscription);
                if (null != subscriptionResponse)
                {
                    // Store the subscription ID so we can keep track of which subscriptions are tied to which users
                    user.SubscriptionId = subscriptionResponse.SubscriptionId;

                    Models.SubscriptionViewModel viewModel = new Models.SubscriptionViewModel { Subscription = subscriptionResponse };
                    return View("Subscription", viewModel);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View("Error");
        }

        private static void FixPPESubscriptionBug(OneDriveUser user, OneDriveSubscription subscription)
        {
            if (user.ClientType == ClientType.Business)
            {
                subscription.Scenarios = null;
       
            }
            else
            {
                subscription.SubscriptionExpirationDateTime = DateTime.Now.AddDays(3);
                subscription.ClientState = null;
            }
        }

        /// <summary>
        /// Delete the user's active subscription and then redirect to logout
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> DeleteSubscription()
        {
            OneDriveUser user = OneDriveUser.UserForRequest(this.Request);
            if (null == user)
            {
                return Redirect(Url.Action("Index", "Home"));
            }

            if (!string.IsNullOrEmpty(user.SubscriptionId))
            {
                var client = await GetOneDriveClientAsync(user);

                // Because the OneDrive SDK does not support OneDrive subscriptions natively yet, 
                // we use BaseRequest to generate a request the SDK can understand
                var request = new BaseRequest(client.BaseUrl + "/drive/root/subscriptions/" + user.SubscriptionId, client) { Method = "DELETE" };

                try
                {
                    var response = await request.SendRequestAsync(null);
                    if (!response.IsSuccessStatusCode)
                    {
                        ViewBag.Message = response.ReasonPhrase;
                        return View("Error");
                    }
                    else
                    {
                        user.SubscriptionId = null;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View("Error");
                }
            }
            return RedirectToAction("SignOut", "Account");
        }

        #region SDK helper methods

        /// <summary>
        /// Create a new instance of the OneDriveClient for the signed in user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static async Task<OneDriveClient> GetOneDriveClientAsync(OneDriveUser user)
        {
            if (string.IsNullOrEmpty(user.OneDriveBaseUrl))
            {
                // Resolve the API URL for this user
                user.OneDriveBaseUrl = await LookupOneDriveUrl(user);
            }

            var client = new OneDriveClient(new AppConfig(), null, null, new OneDriveAccountServiceProvider(user), user.ClientType);
            await client.AuthenticateAsync();

            return client;
        }


        /// <summary>
        /// Use the discovery API to resolve the base URL for the OneDrive API
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static async Task<string> LookupOneDriveUrl(OneDriveUser user)
        {
            if (user.ClientType == ClientType.Consumer)
            {
                return "https://api.onedrive.com/v1.0";
            }
            if (user.ClientType == ClientType.Business)
            {
                return "https://prepsp-my.spoppe.com/_api/v2.0";
            }

            var accessToken = await user.GetAccessTokenAsync("https://api.office.com/discovery/");

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://api.office.com/discovery/v2.0/me/services");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Unable to determine OneDrive URL: " + response.ReasonPhrase);
            }

            var services = JsonConvert.DeserializeObject<DiscoveryServiceResponse>(await response.Content.ReadAsStringAsync());

            var query = from s in services.Value
                        where s.Capability == "MyFiles" && s.ServiceApiVersion == "v2.0"
                        select s.ServiceEndpointUri;

            return query.FirstOrDefault();
        }
        #endregion
    }
}