#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Microsoft.Ajax.Utilities;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// </summary>
    public static class MarketsManager{
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Markets GetById(string id){
            using (var context = new ApplicationDbContext()){
                return context.Markets.AsNoTracking().First(markets => markets.Id.ToString() == id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        public static async Task DeleteMarket(string marketId){
            using (var context = new ApplicationDbContext()){
                var marketData = await context.Markets.FirstAsync(market => market.Id.ToString() == marketId);

                context.Entry(marketData).State = EntityState.Deleted;

                await context.SaveChangesAsync();
            }
        }

        public static IEnumerable<Markets> GetAllMarkets() {
            using (var context = new ApplicationDbContext()) {
                return context.Markets.ToList();
            }
        }

        public static IEnumerable<Markets> GetAllActiveMarkets() {
            using (var context = new ApplicationDbContext()) {
                return context.Markets.Where(_ => _.Active).ToList();
            }
        }

        public static IEnumerable<Markets> GetAllActiveMarketsByShortname(string shortName) {
            using (var context = new ApplicationDbContext()) {
                var coinData = context.CoinSystems.First(__ => __.ShortName == shortName);
                return context.Markets.Where(_ => _.Active && _.CoinFrom == coinData.Id.ToString()).ToList();
            }
        }




        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static HomeViewModels.IndexPageViewModel GetMarketsData(){
            using (var context = new ApplicationDbContext()){
                var coins = context.CoinSystems.AsNoTracking().ToList();
                var allmarkets = context.Markets.AsNoTracking().ToList();

                var bitcoinId = coins.First(coin => coin.Name == "Bitcoin").Id.ToString();
                var litecoinId = coins.First(coin => coin.Name == "Litecoin").Id.ToString();

                var dateFrom = DateTime.UtcNow.AddDays(-1);

                var btcMarketSummary = allmarkets.Where(mrkt => mrkt.CoinFrom == bitcoinId)
                    .Where(btcMarket => context.Orders.AsNoTracking()
                        .Any(orders => orders.MarketId == btcMarket.Id.ToString() && orders.Closed && orders.DateClosed > dateFrom))
                    .Sum(btcMarket => context.Orders.AsNoTracking()
                        .Where(orders => orders.MarketId == btcMarket.Id.ToString() && orders.Closed && orders.DateClosed > dateFrom)
                        .Sum(sum => sum.Total));

                var ltcMarketSummary = allmarkets.Where(mrkt => mrkt.CoinFrom == litecoinId)
                    .Where(ltcMarket => context.Orders.AsNoTracking()
                        .Any(orders => orders.MarketId == ltcMarket.Id.ToString() && orders.Closed && orders.DateClosed > dateFrom))
                    .Sum(ltcMarket => context.Orders.AsNoTracking()
                        .Where(orders => orders.MarketId == ltcMarket.Id.ToString() && orders.Closed && orders.DateClosed > dateFrom)
                        .Sum(sum => sum.Total));




                var marketData = new HomeViewModels.IndexPageViewModel{
                    BtcMarkets = allmarkets.Where(markets => markets.CoinFrom == bitcoinId && markets.Active).OrderBy(mark => mark.PairName).ToList(),
                    LtcMarkets = allmarkets.Where(markets => markets.CoinFrom == litecoinId && markets.Active).OrderBy(mark => mark.PairName).ToList(),
                    BtcValue = btcMarketSummary,
                    LtcValue = ltcMarketSummary,
                    TradesCount = context.Orders.AsNoTracking().Count(orders => orders.Closed && orders.DateClosed > dateFrom)
                };
                return marketData;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetMarketPairName(string id){
            using (var context = new ApplicationDbContext()){
                return context.Markets.AsNoTracking().First(markets => markets.Id.ToString() == id).PairName;
            }
        }

        /// <summary>
        ///     Get closed orders for page
        /// </summary>
        /// <param name="marketId">Market Id</param>
        public static List<Order> GetClosedOrders(string marketId){
            using (var context = new ApplicationDbContext()){
                return context.Orders.AsNoTracking().Where(market => market.MarketId == marketId && market.Closed).OrderByDescending(order => order.DateClosed).Take(10).ToList();
            }
        }

        /// <summary>
        ///     Getting compounded orders data for page
        /// </summary>
        /// <param name="marketId">Market Id</param>
        public static OrdersData GetStructuredOrdersData(string marketId){
            using (var context = new ApplicationDbContext()){
                // Get all orders with MarketId, that not closed still
                var rawOrders =  context.Orders.AsNoTracking().Where(order => order.MarketId == marketId).ToList();
                // Get all unique by "Price Per One" sell orders 
                var sellUniqueOrders = rawOrders.Where(rord => rord.TradeType == TradeType.Sell && !rord.Closed && rord.PartialOrderTotalLeft > 0).OrderBy(buyOrderBy => buyOrderBy.Price).ToList();
                // Compound sell orders with raw data
                var compoundSellOrders = sellUniqueOrders.DistinctBy(dbby => dbby.Price).Select(sellOrderWithUniquePrice => new Order{
                    Price = sellOrderWithUniquePrice.Price,
                    PartialOrderTotalLeft = sellUniqueOrders.Where(order => order.Price == sellOrderWithUniquePrice.Price).Sum(price => price.PartialOrderTotalLeft),
                    Total = sellUniqueOrders.Where(order => order.Price == sellOrderWithUniquePrice.Price).Sum(price => price.Total)
                }).ToList();

                // Get all unique by "Price Per One" buy orders 
                var buyUniqueOrders = rawOrders.Where(rord => rord.TradeType == TradeType.Buy && !rord.Closed && rord.PartialOrderTotalLeft > 0).OrderByDescending(buyOrderBy => buyOrderBy.Price).ToList();
                // Compound buy orders with raw data
                var compoundBuyOrders = buyUniqueOrders.DistinctBy(sbby => sbby.Price).Select(buyOrderWithUniquePrice => new Order{
                    Price = buyOrderWithUniquePrice.Price,
                    PartialOrderTotalLeft = buyUniqueOrders.Where(order => order.Price == buyOrderWithUniquePrice.Price).Sum(price => price.PartialOrderTotalLeft),
                    Total = buyUniqueOrders.Where(order => order.Price == buyOrderWithUniquePrice.Price).Sum(price => price.Total)
                }).ToList();

                double high = 0, low = 0;
                var pastDay = DateTime.UtcNow.AddDays(-1);
                if (rawOrders.Where(order => order.Closed && order.DateClosed.Value > pastDay).ToList().Count > 0){
                    high = rawOrders.Where(order => order.Closed && order.DateClosed.Value > pastDay).ToList().Max(order => order.Price);
                    low = rawOrders.Where(order => order.Closed && order.DateClosed.Value > pastDay).ToList().Min(order => order.Price);
                }

                var comSellOrders = compoundSellOrders.Where(order => order.PartialOrderTotalLeft > 0).ToList();
                var comBuyOrders = compoundBuyOrders.Where(order => order.PartialOrderTotalLeft > 0).ToList();

                // Return structured data
                return new OrdersData{
                    SellOrders = comSellOrders,
                    BuyOrders = comBuyOrders,
                    DayHigh = high,
                    DayLow = low,
                    TotalBuy = comBuyOrders.Sum(_=>_.PartialOrderTotalLeft),
                    TotalSell = comSellOrders.Sum(_=>_.PartialOrderTotalLeft)
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class OrdersData{
            /// <summary>
            /// 
            /// </summary>
            public List<Order> BuyOrders { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<Order> SellOrders { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DayHigh { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DayLow { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DayVolume { get; set; }

            public double TotalBuy { get; set; }

            public double TotalSell { get; set; }
        }
    }
}