using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveWebhookTranslator.Models
{
    /// <summary>
    /// This class manages an in-memory set of users. In a production serivce this class would
    /// be provided by a backend database that could manage the state of users securely across 
    /// multiple server instances.
    /// </summary>
    class OneDriveUserManager
    {
        private static Dictionary<string, OneDriveUser> KnownUsers = new Dictionary<string, OneDriveUser>();

        public static OneDriveUser LookupUserById(string userGuid)
        {
            OneDriveUser user;
            if (KnownUsers.TryGetValue(userGuid, out user))
                return user;

            throw new InvalidOperationException("Unknown user.");
        }

        public static void RegisterUser(string userGuid, OneDriveUser user)
        {
            KnownUsers[userGuid] = user;
        }

        public static OneDriveUser LookupUserForSubscriptionId(string subscriptionId)
        {
            var query = from u in KnownUsers.Values
                        where u.SubscriptionId == subscriptionId
                        select u;

            return query.FirstOrDefault();
        }

    }
}
