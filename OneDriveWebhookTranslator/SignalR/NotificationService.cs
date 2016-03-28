using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Linq;

namespace OneDriveWebhookTranslator.SignalR
{
    public class NotificationService : PersistentConnection
    {
        public void SendNotificationToClient(List<Models.OneDriveWebhookNotification> items)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            if (hubContext != null)
            {
                hubContext.Clients.All.showNotification(items);
            }
        }

        public void SendFileChangeNotification(List<string> filenames)
        {
            if (null == filenames || !filenames.Any())
                return;


            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            if (null != hubContext)
            {
                hubContext.Clients.All.filesChanged(filenames);
            }
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            return Connection.Send(connectionId, "Connected");
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }
    }

}