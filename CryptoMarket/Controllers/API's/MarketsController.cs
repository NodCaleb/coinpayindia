#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// Market RESTful API
    /// </summary>
    public class MarketsController : ApiController{
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        /// <summary>
        /// Getting list of currently active markets
        /// </summary>
        /// <returns>List of currently active markets</returns>
        public IEnumerable<Markets> Get(){
            return _db.Markets.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        public class ApiMarkets{
            /// <summary>
            /// 
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Pair { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double LatestPrice { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DailyLow { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DailyHigh { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DailyVolume { get; set; }

        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing){
            if (disposing){
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}