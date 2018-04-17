using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CryptoMarket.Models;
using Microsoft.AspNet.SignalR;

namespace CryptoMarket.Source.Managers {
    /// <summary>
    /// 
    /// </summary>
    public class ChatManager {

        /// <summary>
        /// 
        /// </summary>
        public static List<ChatItem> Conversation = new List<ChatItem>();
        /// <summary>
        /// 
        /// </summary>
        public class ChatItem {
            /// <summary>
            /// 
            /// </summary>
            public string Username { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DateTime { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="message"></param>
        public static void AddChatMessage(string userid, string message) {
            using (var context = new ApplicationDbContext()) {
                var username = context.Users.First(_ => _.Id == userid).UserName;
                Conversation.Add(new ChatItem {
                    DateTime = DateTime.UtcNow.ToShortTimeString(),
                    Message = message,
                    Username = username
                });

                //try {
                    GlobalHost.ConnectionManager.GetHubContext<MarketrealtimeHub>()
                        .Clients.All.chatMessage(username, message, DateTime.UtcNow.ToShortTimeString());
                //}
                //catch {
                    // do nothing
                //}
            }
        }
    }
}