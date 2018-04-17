using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

namespace CryptoMarket.Controllers {
    /// <summary>
    /// 
    /// </summary>
    public class CoinsController : ApiController {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        /// <summary>
        /// Getting list of all coins on exchange
        /// </summary>
        /// <returns>List of coins</returns>
        public IEnumerable<CoinApiResponse> Get() {
            var returnData = _db.CoinSystems.Select(coin => new CoinApiResponse {
                    Id = coin.Id.ToString(),
                    Abbreviation = coin.ShortName,
                    Name = coin.Name,
                    Active = coin.Active,
                    DepositDisabled = coin.DepositDisabled,
                    WithdrawDisabled = coin.WithdrawDisabled
                })
                .ToList();

            return returnData;
        }

        /// <summary>
        /// Coin api response structure 
        /// </summary>
        public class CoinApiResponse {

            public string Id { get; set; }
            public string Name { get; set; }

            public string Abbreviation { get; set; }

            public bool Active { get; set; }

            public bool DepositDisabled { get; set; }

            public bool WithdrawDisabled { get; set; }
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
