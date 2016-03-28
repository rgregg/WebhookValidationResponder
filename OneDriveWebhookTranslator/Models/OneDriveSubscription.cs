using System;
using Newtonsoft.Json;

namespace OneDriveWebhookTranslator.Models
{
    public class OneDriveSubscription
    {
        // The string that MS Graph should send with each notification. Maximum length is 255 characters. 
        // To verify that the notification is from MS Graph, compare the value received with the notification to the value you sent with the subscription request.
        [JsonProperty("clientState", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ClientState { get; set; }

        // The URL of the endpoint that receives the subscription response and notifications. Requires https.
        [JsonProperty("notificationUrl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string NotificationUrl { get; set; }

        // The resource to monitor for changes.
        [JsonProperty("resource", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Resource { get; set; }

        // The date and time when the webhooks subscription expires.
        // The time is in UTC, and can be up to three days from the time of subscription creation.
        [JsonProperty("expirationDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset SubscriptionExpirationDateTime { get; set; }

        // The unique identifier for the webhooks subscription.
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SubscriptionId { get; set; }

        // OneDrive Personal requires scenarios to be passed currently. This requirement will be removed in the future
        [JsonProperty("scenarios", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Scenarios { get; set; }

        public OneDriveSubscription()
        {
            this.Scenarios = new string[] { "Webhook" };
        }
    }

    public class SubscriptionViewModel
    {
        public OneDriveSubscription Subscription { get; set; }
    }
}