using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using CryptoMarket.Models;

namespace CryptoMarket.Controllers {
    /// <summary>
    /// 
    /// </summary>
    public class CmcTickerController : ApiController {

        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        // GET: api/CmcTicker
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CmcTickerResponse>> Get() {
            var response = new List<CmcTickerResponse>();
            var markets = await _db.Markets.ToListAsync();
            var dateFrom = DateTime.UtcNow.AddDays(-1);
            foreach (var market in markets) {
                var volume = markets.Where(_ => _.Id == market.Id)
                    .Where(btcMarket => _db.Orders.AsNoTracking()
                        .Any(orders => orders.MarketId == btcMarket.Id.ToString() && orders.Closed && orders.DateClosed > dateFrom))
                    .Sum(btcMarket => _db.Orders.AsNoTracking()
                        .Where(orders => orders.MarketId == btcMarket.Id.ToString() && orders.Closed && orders.DateClosed > dateFrom)
                        .Sum(sum => sum.Total));
                response.Add(new CmcTickerResponse {
                   PairName = market.PairName,
                   Volume24 = volume,
                   LastPrice = market.LatestPrice.Value
               });    
            }
            return response;
        }

        /// <summary>
        /// CoinMarketCap Required info
        /// </summary>
        public class CmcTickerResponse {
            /// <summary>
            /// 
            /// </summary>
            public string PairName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Volume24 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double LastPrice { get; set; }
        }


        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) { _db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
