#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Core;

#endregion

namespace CryptoMarket.Source.Managers {
    /// <summary>
    /// 
    /// </summary>
    public static class OrdersManager {
        private static object _lock = new object();

        public class OrderCreateResult {
            public bool Success { get; set; }

            public Order OrderResult { get; set; }

            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// Creation of new Order on local exchange
        /// </summary>
        /// <param name="marketId"></param>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <param name="orderType"></param>
        /// <param name="tradeType"></param>
        /// <param name="actionPrice"></param>
        /// <returns></returns>
        public static OrderCreateResult Create(string marketId, string userId, double amount, double price, OrderTypes orderType, TradeType tradeType, double actionPrice) {
            lock (_lock) {
                using (var context = new ApplicationDbContext()) {
                    var marketInfo = context.Markets.First(market => market.Id.ToString() == marketId);

                    if (!marketInfo.Active) {
                        return new OrderCreateResult {
                            Success = false,
                            ErrorMessage = "Market deactivated"
                        };
                    }

                    var minimumTradeAmount = context.CoinSystems.First(coin => coin.Id.ToString() == marketInfo.CoinTo).MinimumTradeAmount;
                    if (minimumTradeAmount > amount) {
                        return new OrderCreateResult {
                            Success = false,
                            ErrorMessage = $"Minimum trade Amount: {minimumTradeAmount}"
                        };
                    }

                    if (orderType == OrderTypes.Market || orderType == OrderTypes.StopLossMarket || orderType == OrderTypes.TakeProfitMarket) {
                        price = marketInfo.LatestPrice.Value;
                    }

                    if (amount <= 0 || price <= 0) {
                        return new OrderCreateResult {
                            Success = false,
                            ErrorMessage = "Amount and Price must be greather zero"
                        };
                    }

                    var total = Math.Round(Math.Round(amount, 8) * Math.Round(price, 8), 8);

                    if (total <= 0.0001) {
                        return new OrderCreateResult {
                            Success = false,
                            ErrorMessage = "Total Amount MUST be greather than 0.0001"
                        };
                    }

                    var feeTotal = total * marketInfo.Fee / 100;
                    var netTotal = Math.Round(total + feeTotal, 8);



                    if (tradeType == TradeType.Sell) {
                        var userBalance = context.Balances.First(balance => balance.UserId == userId && balance.CoinId == marketInfo.CoinTo);
                        if (userBalance.Balance - amount < 0) {
                            return new OrderCreateResult {
                                Success = false,
                                ErrorMessage = "Not enought coins for order creation. Check your balance."
                            };
                        }
                        new BalancesManager(context).Withdraw(userId, marketInfo.CoinTo, amount);
                    } else {
                        var userBalance = context.Balances.First(balance => balance.UserId == userId && balance.CoinId == marketInfo.CoinFrom);
                        if (userBalance.Balance - total < 0)
                        {
                            return new OrderCreateResult
                            {
                                Success = false,
                                ErrorMessage = "Not enought coins for order creation. Check your balance."
                            };
                        }
                        new BalancesManager(context).Withdraw(userId, marketInfo.CoinFrom, total);
                    }


                    var orderCreate = new Order {
                        UserId = userId,
                        DateCreated = DateTime.UtcNow,
                        Amount = amount,
                        Price = price,
                        Total = total,
                        FeeTotal = feeTotal,
                        NetTotal = netTotal,
                        TradeType = tradeType,
                        MarketId = marketId,
                        PartialOrderTotalLeft = amount,
                        OrderType = orderType,
                        ActionPrice = actionPrice
                    };

                    context.Orders.Add(orderCreate);

                    context.SaveChanges();


                    SingalRManager.NewSellBuyOrdersSendToAll(marketId);

                    return new OrderCreateResult {
                        Success = true,
                        OrderResult = orderCreate
                    };

                }
            }
        }


        /// <summary>
        ///     Async Getting users active orders by market
        /// </summary>
        /// <param name="marketId">Market Id</param>
        /// <param name="userId">User Id</param>
        /// <returns></returns>
        public static List<Order> GetUserActiveOrdersByMarketIdAsync(string marketId, string userId) {
            using (var context = new ApplicationDbContext()) {
                return  context.Orders.Where(usr => usr.UserId == userId && !usr.Closed && usr.MarketId == marketId).AsNoTracking().ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="market"></param>
        /// <returns></returns>
        public static List<Order> GetOrders(string market = null) {
            using (var context = new ApplicationDbContext()) {
                if (market != null) {
                    var markets = context.Markets.AsNoTracking().First(_ => _.Id.ToString() == market);
                    return  context.Orders.Where(_ => _.MarketId == markets.Id.ToString() && _.Closed == false).AsNoTracking().ToList();
                }

                return  context.Orders.Where(_ => _.Closed == false).AsNoTracking().ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static async Task<Order> Get(string orderId) {
            using (var context = new ApplicationDbContext()) {
                return await context.Orders.FirstAsync(usr => usr.Id.ToString() == orderId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        public static async Task DeleteAllMarketOrders(string marketId) {
            using (var context = new ApplicationDbContext()) {
                var orders = await context.Orders.Where(order => order.MarketId == marketId).ToListAsync();
                foreach (var order in orders) {
                    Delete(order.Id.ToString(), order.UserId);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool Delete(string orderId, string userId) {

            using (var context = new ApplicationDbContext()) {
                if (context.Orders.Any(orders => orders.Id.ToString() == orderId && orders.UserId == userId && !orders.Closed)) {
                    var orderData = context.Orders.First(orders => orders.Id.ToString() == orderId && orders.UserId == userId && !orders.Closed);
                    var marketData = context.Markets.First(markets => markets.Id.ToString() == orderData.MarketId);

                    context.Entry(orderData).State = EntityState.Deleted;

                    // Dependency Injection for balance Updating
                    var balanceManager = new BalancesManager(context);

                    switch (orderData.TradeType) {
                        case TradeType.Buy:
                            balanceManager.Deposit(userId, marketData.CoinFrom, orderData.PartialOrderTotalLeft);
                            break;
                        case TradeType.Sell:
                            balanceManager.Deposit(userId, marketData.CoinTo, orderData.PartialOrderTotalLeft);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    context.SaveChanges();

                    SingalRManager.NewSellBuyOrdersSendToAll(orderData.MarketId);

                    return true;
                }
                return false;

            }
        }
    }
}