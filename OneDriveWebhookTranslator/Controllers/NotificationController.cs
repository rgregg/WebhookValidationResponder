using Newtonsoft.Json;
using OneDriveWebhookTranslator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.OneDrive.Sdk;

namespace OneDriveWebhookTranslator.Controllers
{
    public class NotificationController : Controller
    {
        public ActionResult LoadView(string subscriptionId)
        {
            ViewBag.SubscriptionId = subscriptionId;
            return View("Notification");
        }

        /// <summary>
        /// Parse JSON from webhook message
        /// </summary>
        /// <returns></returns>
        private async Task<Models.OneDriveWebhookNotification[]> ParseIncomingNotificationAsync()
        {
            try
            {
                using (var inputStream = new System.IO.StreamReader(Request.InputStream))
                {
                    var collection = JsonConvert.DeserializeObject<Models.OneDriveNotificationCollection>(await inputStream.ReadToEndAsync());
                    if (collection != null && collection.Notifications != null)
                    {
                        return collection.Notifications;
                    }
                }
            }
            catch { }
            return null;
        }

        public async Task<ActionResult> Listen()
        {
            #region Validation new subscriptions
            // Respond to validation requests from the service by sending the token
            // back to the service. This response is required for each subscription.
            const string ValidationTokenKey = "validationToken";
            if (Request.QueryString[ValidationTokenKey] != null)
            {
                string token = Request.QueryString[ValidationTokenKey];
                return Content(token, "text/plain");
            }
            #endregion

            var notifications = await ParseIncomingNotificationAsync();
            if (null != notifications && notifications.Any())
            {
            }

            // Return a 200 so the service doesn't resend the notification.
            return new HttpStatusCodeResult(200);
        }
    }
}
