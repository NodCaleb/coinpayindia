#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

#endregion

namespace CryptoMarket.Source {


    /// <summary>
    /// 
    /// </summary>
    [HubName("messagesrealtime")]
    public class MessagesrealtimeHub : Hub {
        /// <summary>
        /// 
        /// </summary>
        public static readonly Dictionary<string, string> MarketRealConnectionUserList = new Dictionary<string, string>(1024);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="senderUserId"></param>
        public void JoinConversation(string senderUserId) {
            Groups.Add(Context.ConnectionId, senderUserId);
        }

        /// <summary>
        /// Add new message via SignalR
        /// </summary>
        /// <param name="senderUserId"></param>
        /// <param name="recipientUserId"></param>
        /// <param name="message"></param>
        public void AddMessage(string senderUserId, string recipientUserId, string message) {
            using (var context = new ApplicationDbContext()) {
                var pm = new PersonalMessagesManager(context);

                pm.SendMessage(senderUserId, recipientUserId, message);

                Clients.Group(recipientUserId).newMessage(message);
            }
        }
    }
}