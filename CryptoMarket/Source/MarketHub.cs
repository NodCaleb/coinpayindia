#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

#endregion

namespace CryptoMarket.Source {
    /// <summary>
    /// 
    /// </summary>
    public class MarketRealConnectionData {
        /// <summary>
        /// 
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [HubName("marketrealtime")]
    public class MarketrealtimeHub : Hub {
        /// <summary>
        /// 
        /// </summary>
        public static readonly Dictionary<string, string> MarketRealConnectionUserList = new Dictionary<string, string>(1024);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        public void MarketHello(string marketId) {
            Groups.Add(Context.ConnectionId, marketId);

            SingalRManager.NewSellBuyOrdersSendToAll(marketId);
        }

        /// <inheritdoc />
        public override Task OnConnected() {
            var con = Context.Request.GetHttpContext();
            var user = con.User;
            var identity = user.Identity;
            var userId = identity.GetUserId();

            if (userId == null)
                return base.OnConnected();

            if (MarketRealConnectionUserList.Any(p => p.Key == userId))
                MarketRealConnectionUserList.Remove(userId);

            MarketRealConnectionUserList.Add(userId, Context.ConnectionId);
            return base.OnConnected();
        }

        /// <summary>
        /// Called when the connection reconnects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
        /// </returns>
        public override Task OnReconnected() {
            var userId = Context.Request.GetHttpContext().User.Identity.GetUserId();

            if (userId == null)
                return base.OnReconnected();

            if (MarketRealConnectionUserList.Any(p => p.Key == userId))
                MarketRealConnectionUserList.Remove(userId);

            MarketRealConnectionUserList.Add(userId, Context.ConnectionId);
            return base.OnReconnected();
        }
    }
}