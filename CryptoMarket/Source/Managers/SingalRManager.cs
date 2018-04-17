#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

#endregion

namespace CryptoMarket.Source.Managers {
    /// <summary>
    /// 
    /// </summary>
    public static class SingalRManager {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <param name="marketId"></param>
        public static void OrderClosedSendToClient(string userId, string message, string marketId) {
            try {
                if (MarketrealtimeHub.MarketRealConnectionUserList.Any(mrc => mrc.Key == userId)) {
                    GlobalHost.ConnectionManager.GetHubContext<MarketrealtimeHub>().Clients.Client(MarketrealtimeHub.MarketRealConnectionUserList.First(mrc => mrc.Key == userId).Value).orderClosed(message);
                }
            } catch {
                // Do nothing
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        public static void SendMessage(string username, string message) {
            try {
                GlobalHost.ConnectionManager.GetHubContext<MarketrealtimeHub>().Clients.All.chatMessage(username, message, DateTime.UtcNow.ToShortTimeString());
            } catch {
                // do nothing
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="coinId"></param>
        /// <param name="balance"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void BalanceUpdateSendToClient(string userId, string coinId, double balance, double value, BalancesManager.BalanceUpdateType type) {
            try {
                if (MarketrealtimeHub.MarketRealConnectionUserList.Any(mrc => mrc.Key == userId)) {
                    GlobalHost.ConnectionManager.GetHubContext<MarketrealtimeHub>().Clients.Client(MarketrealtimeHub.MarketRealConnectionUserList.First(mrc => mrc.Key == userId).Value).balanceUpdate(coinId, balance.ToString("F8"), CoinsManager.GetCoinNameById(coinId), value, type);
                }
            } catch {
                // ignored
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        public static void NewSellBuyOrdersSendToAll(string marketId) {
            // try{
            var data = MarketsManager.GetStructuredOrdersData(marketId);
            var history = MarketsManager.GetClosedOrders(marketId);

            var depthGraph = data.BuyOrders.Select(buyOrders => new DepthGraphData {
                Price = buyOrders.Price,
                Buy = buyOrders.Total
            }).ToList();

            depthGraph.AddRange(data.SellOrders.Select(sellOrders => new DepthGraphData {
                Price = sellOrders.Price,
                Sell = sellOrders.Total
            }));

            try {

                GlobalHost.ConnectionManager.GetHubContext<MarketrealtimeHub>().Clients.Group(marketId).updateBuySell(data, history, depthGraph);
            } catch {
                // ignored
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DepthGraphData {
            /// <summary>
            /// 
            /// </summary>
            public double Price { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Buy { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Sell { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <param name="priceChangePercent"></param>
        /// <param name="price"></param>
        /// <param name="grow"></param>
        public static void PriceChangedSendToAll(string marketId, double priceChangePercent, double price, bool grow) {
            var formattedPrice = price.ToString("F8");
            var formattedPriceChangePercent = $"{priceChangePercent}%{(grow ? "▲" : "▼")}";
            try {
                GlobalHost.ConnectionManager.GetHubContext<MarketrealtimeHub>().Clients.All.priceChanged(marketId, formattedPriceChangePercent, formattedPrice, grow);
            } catch {
                // ignored
            }
        }
    }
}