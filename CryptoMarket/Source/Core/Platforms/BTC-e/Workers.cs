using System;
using System.Linq;
using BtcE;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Quartz;

namespace CryptoMarket.Source.Core.Platforms{
    /// <summary>
    /// 
    /// </summary>
    public class Workers{
        /// <summary>
        /// 
        /// </summary>
        public class OrdersJob : IJob{

            /// <summary>
            /// Execution Interval in Seconds
            /// </summary>
            public static int ExecutionIntervalSeconds = 2;

            public void Execute(IJobExecutionContext jobContext){
                var btceTickers = BtceApiV3.GetTicker(new[]{
                    BtcePair.btc_usd,
                    BtcePair.btc_eur,
                    BtcePair.btc_rur,
                    BtcePair.eur_usd,
                    BtcePair.ftc_btc,
                    BtcePair.ltc_btc,
                    BtcePair.ltc_eur,
                    BtcePair.ltc_rur,
                    BtcePair.ltc_usd,
                    BtcePair.nmc_btc,
                    BtcePair.nmc_usd,
                    BtcePair.nvc_btc,
                    BtcePair.nvc_usd,
                    BtcePair.ppc_btc,
                    BtcePair.trc_btc,
                    BtcePair.usd_rur,
                    BtcePair.xpm_btc
                });

                using (var context = new ApplicationDbContext()){
                    var orders = context.BtceOrders.AsNoTracking().Where(_ => _.Type == OrderTypes.StopLossMarket || _.Type == OrderTypes.TakeProfitMarket || _.Type == OrderTypes.StopLossLimit || _.Type == OrderTypes.TakeProfitLimit && (_.State == OrderState.Opened || _.State == OrderState.PartialClosed));

                    foreach (var order in orders){

                        var ticker = btceTickers.First(_ => _.Key == order.Pair).Value;
                        var latestPrice = ticker.Last;

                        switch (order.Type){
                            case OrderTypes.StopLossMarket:

                                if (latestPrice <= order.StopProfitPrice){
                                    if (order.TradeType == TradeType.Buy){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Buy, order.Amount, ticker.Sell);
                                    }
                                    else if (order.TradeType == TradeType.Sell){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Sell, order.Amount, ticker.Buy);
                                    }
                                }
                                break;

                            case OrderTypes.TakeProfitMarket:

                                if (latestPrice >= order.StopProfitPrice){
                                    if (order.TradeType == TradeType.Buy){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Buy, order.Amount, ticker.Sell);
                                    }
                                    else if (order.TradeType == TradeType.Sell){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Sell, order.Amount, ticker.Buy);
                                    }
                                }
                                break;


                            case OrderTypes.StopLossLimit:

                                if (latestPrice <= order.StopProfitPrice)
                                {
                                    if (order.TradeType == TradeType.Buy){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Buy, order.Amount, order.Price);
                                    }
                                    else if (order.TradeType == TradeType.Sell){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Sell, order.Amount, order.Price);
                                    }
                                }

                                break;


                            case OrderTypes.TakeProfitLimit:

                                if (latestPrice >= order.StopProfitPrice){
                                    if (order.TradeType == TradeType.Buy){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Buy, order.Amount, order.Price);
                                    }
                                    else if (order.TradeType == TradeType.Sell){
                                        PlaceOrder(order.Pair, order.UserId, TradeType.Sell, order.Amount, order.Price);
                                    }
                                }
                                break;

                            default:
                                throw new ArgumentException();
                        }
                    }
                }

            }

            private static void PlaceOrder(BtcePair pair, string userId, TradeType tradeType, decimal amount, decimal price){
                using (var context = new ApplicationDbContext()){
                    var userInfo = context.Users.First(_ => _.Id == userId);
                    var btceApi = new BtceApiClientV3(userInfo.BtceKey, userInfo.BtceSecret);

                    try{
                        btceApi.Trade(pair, tradeType, price, amount);
                    }
                    catch (BtceException){}
                }
            }
        }
    }
}