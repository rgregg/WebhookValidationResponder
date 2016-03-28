using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneDriveWebhookTranslator.Models
{
    public class OneDriveWebhookNotification
    {
        // The client state used to verify that the notification is from Microsoft Graph. Compare the value received with the notification to the value you sent with the subscription request.
        [JsonProperty("clientState")]
        public string ClientState { get; set; }

        // The endpoint of the resource that changed. For example, a message uses the format ../Users/{user-id}/Messages/{message-id}
        [JsonProperty("resource")]
        public string Resource { get; set; }

        // The date and time when the webhooks subscription expires.
        // The time is in UTC, and can be up to three days from the time of subscription creation.
        [JsonProperty("subscriptionExpirationDateTime")]
        public string SubscriptionExpirationDateTime { get; set; }

        // The unique identifier for the webhooks subscription.
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("receivedDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime ReceivedDateTime { get; set; }

        public OneDriveWebhookNotification()
        {
            this.ReceivedDateTime = DateTime.UtcNow;
        }

    }

    public class OneDriveNotificationCollection
    {
        [JsonProperty("value")]
        public OneDriveWebhookNotification[] Notifications { get; set; }

    }
}