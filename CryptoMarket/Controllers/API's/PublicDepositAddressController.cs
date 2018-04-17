using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using CryptoMarket.Models;
using CryptoMarket.Source.Managers;
using Microsoft.Ajax.Utilities;

namespace CryptoMarket.Controllers{
    /// <summary>
    /// 
    /// </summary>
    public class PublicDepositAddressController : ApiController{
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Route("api/publicdepositaddress/{username}")]
        public UserPublicDepositInfo Get(string username){
            var userId = _db.Users.AsNoTracking().FirstOrDefault(user => user.UserName == username)?.Id;
            var coins = _db.CoinSystems.AsNoTracking().ToList();
            var userBalances = _db.Balances.AsNoTracking().Where(_ => _.UserId == userId);

            var retData = new UserPublicDepositInfo{
                Username = username,
                Coins = new List<UserPublicDepositInfo.CoinAddress>()
            };

            foreach (var userDepositAddress in _db.DepositAddressesAssociations.AsNoTracking().Where(user => user.UserId == userId).DistinctBy(dis=>dis.CoinId).ToList().Where(userDepositAddress => coins.Any(_ => _.Id.ToString() == userDepositAddress.CoinId))){
                retData.Coins.Add(new UserPublicDepositInfo.CoinAddress {
                    Name = CoinsManager.GetCoinNameById(userDepositAddress.CoinId),
                    Address = userDepositAddress.Address,
                    Balance = userBalances.First(_=>_.CoinId == userDepositAddress.CoinId).Balance.ToString("N8")
                });
            }
            return retData;
        }

        /// <summary>
        /// 
        /// </summary>
        public class UserPublicDepositInfo{
            /// <summary>
            /// User Name
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<CoinAddress> Coins { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public class CoinAddress{
                /// <summary>
                /// Coin Name
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// User Deposit Address
                /// </summary>
                public string Address { get; set; }

                /// <summary>
                /// 
                /// </summary>
                public string Balance { get; set; }
            }

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