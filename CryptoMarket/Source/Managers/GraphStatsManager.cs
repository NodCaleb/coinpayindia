#region

using System;
using System.Data.Entity;
using System.Linq;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Newtonsoft.Json;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// 
    /// </summary>
    public static class GraphStatsManager{
        /// <summary>
        ///     Thread-safe stats comparsion.
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <param name="matchedPrice"></param>
        /// <param name="total"></param>
        public static void Write(string marketId, double matchedPrice, double total){
            // Lock code parallel execute to better stats accuracy

                using (var context = new ApplicationDbContext()){
                    // This will set period of stats by 1 hour
                    var currentPeriod = RoundUp(DateTime.UtcNow, TimeSpan.FromHours(1));
                    // If we don't have basic info for current period, filling it with
                    if (!context.GraphStats.Any(graphData => graphData.MarketId == marketId && graphData.DateTimePeriod == currentPeriod)){
                        // Add new data. All (OPEN,HIGH,LOW,CLOSE) are similar to each one, cuz we have only one transaction
                        // OPEN value is NOT changing anymore.
                        context.GraphStats.Add(new GraphStats{
                            DateTimePeriod = currentPeriod,
                            Open = matchedPrice,
                            High = matchedPrice,
                            Low = matchedPrice,
                            Close = matchedPrice,
                            MarketId = marketId,
                            Volume = total
                        });
                        // Save new item to DB
                        context.SaveChanges();
                    }
                    // Get all info about current period
                    var currentPeriodGraphData = context.GraphStats.First(graphData => graphData.MarketId == marketId && graphData.DateTimePeriod == currentPeriod);
                    // We setting up CLOSE value each time, cuz this entry can be the last in this period. 
                    currentPeriodGraphData.Close = matchedPrice;

                    currentPeriodGraphData.Low = matchedPrice < currentPeriodGraphData.Low ? matchedPrice : currentPeriodGraphData.Low;
                    currentPeriodGraphData.High = matchedPrice > currentPeriodGraphData.High ? matchedPrice : currentPeriodGraphData.High;

                    currentPeriodGraphData.Volume += total;

                    // Set modified state
                    context.Entry(currentPeriodGraphData).State = EntityState.Modified;
                    context.SaveChanges();

                    //Price Graph Fill Data
                    var currentPricePeriod = RoundUp(DateTime.UtcNow, TimeSpan.FromMinutes(1));
                    if (!context.PriceGraphs.Any(price => price.MarketId == marketId && price.Period == currentPricePeriod)) {
                        context.PriceGraphs.Add(new PriceGraph {
                            MarketId = marketId,
                            Period = currentPricePeriod,
                            Price = matchedPrice
                        });
                        context.SaveChanges();
                    } else {
                        var priceData = context.PriceGraphs.First(price => price.MarketId == marketId && price.Period == currentPricePeriod);
                        priceData.Price = matchedPrice;
                        context.Entry(priceData).State = EntityState.Modified;
                        context.SaveChanges();
                    } 

                    // Get current market data
                    var marketData = context.Markets.First(markt => markt.Id.ToString() == marketId);

                    marketData.LatestPrice = matchedPrice;

                    // Getting average data from past
                    var dateFromQuery = DateTime.UtcNow.AddHours(-24);
                    var orderFromPast = context.Orders.Where(order => order.MarketId == marketId && order.DateClosed < DateTime.UtcNow || order.DateClosed > dateFromQuery).OrderByDescending(date => date.DateClosed).Skip(1).ToList();
                    var averageFromPast = 0.0;
                    if (orderFromPast.Count > 0)
                        averageFromPast = orderFromPast.Average(avg => avg.Price);

                    // Getting change percent
                    var changePercent = 0.0;
                    if (averageFromPast > 0 && matchedPrice > 0)
                        changePercent = -Math.Round((averageFromPast - matchedPrice)/averageFromPast, 2);

                    if (marketData.PriceChangePercent != changePercent){
                        var grow = marketData.PriceChangePercent < changePercent;
                        SingalRManager.PriceChangedSendToAll(marketId, changePercent, matchedPrice, grow);
                    }

                    marketData.PriceChangePercent = changePercent;

                    // Set modified state
                    context.Entry(marketData).State = EntityState.Modified;
                    // Save changes to DB
                    context.SaveChanges();
                }
            
        }

        public static GraphStats GetStat(string marketId)
        {
            using (var context = new ApplicationDbContext())
            {
                // var startPeriod = RoundUp(DateTime.UtcNow.AddHours(-24), TimeSpan.FromHours(1));
                // var currentPeriod = RoundUp(DateTime.UtcNow, TimeSpan.FromHours(1));
                //var utcPeriod = DateTime.UtcNow.Date

                // var stata = context.GraphStats.Where(graphData => graphData.MarketId == marketId && graphData.DateTimePeriod >= startPeriod).ToList();

                var startTime = DateTime.UtcNow.AddHours(-24);

                var orders = context.Orders.Where(_ => _.MarketId == marketId).Where(_ => _.Closed).Where(_ => _.DateClosed > startTime).ToList();

                if (!orders.Any())
                {
                    return new GraphStats
                    {
                        Close = 0,
                        DateTimePeriod = startTime,
                        High = 0,
                        Low = 0,
                        MarketId = marketId,
                        Open = 0,
                        Volume = 0
                    };
                }


                return new GraphStats
                {
                    Close = orders.Last().Price,
                    DateTimePeriod = startTime,
                    High = orders.Max(_ => _.Price),
                    Low = orders.Min(_ => _.Price),
                    MarketId = marketId,
                    Open = orders.First().Price,
                    Volume = orders.Sum(_ => _.Amount)
                };



            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        public static string Get(string marketId){
            using (var context = new ApplicationDbContext()){
                // Get data for current market from DB and
                var returnData = context.GraphStats.Where(graph => graph.MarketId == marketId).OrderBy(date => date.DateTimePeriod).ToList().Select(graphEntry => new OutputGraphFormat{
                    Period = graphEntry.DateTimePeriod.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                    Low = graphEntry.Low.ToString("N8"),
                    Open = graphEntry.Open.ToString("N8"),
                    Close = graphEntry.Close.ToString("N8"),
                    High = graphEntry.High.ToString("N8"),
                    Volume = graphEntry.Volume.ToString("N8")
                }).ToList();
                return JsonConvert.SerializeObject(returnData);
            }
        }

        public static string GetPrice(string marketId) {
            using (var context = new ApplicationDbContext()) {
                // Get data for current market from DB and
                var returnData = context.PriceGraphs.Where(graph => graph.MarketId == marketId).OrderBy(date => date.Period).AsNoTracking().ToList().Select(graphEntry => new OutputGraphPriceFormat {
                    Period = graphEntry.Period.AddHours(9).ToString("u"),
                    Price = graphEntry.Price.ToString("N8")
                }).ToList();
                return JsonConvert.SerializeObject(returnData);
            }
        }

        private class OutputGraphPriceFormat {
            public string Period { get; set; }

            public string Price { get; set; }
        }

        private static DateTime RoundUp(DateTime dt, TimeSpan d){
            return new DateTime(((dt.Ticks + d.Ticks - 1)/d.Ticks)*d.Ticks);
        }

        private class OutputGraphFormat{
            public string Period { get; set; }
            public string Low { get; set; }
            public string Open { get; set; }
            public string Close { get; set; }
            public string High { get; set; }
            public string Volume { get; set; }
        }
    }
}