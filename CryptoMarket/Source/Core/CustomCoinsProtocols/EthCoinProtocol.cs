using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Nethereum.Web3;

namespace CryptoMarket.Source.Core.CustomCoinsProtocols {
    /// <summary>
    /// 
    /// </summary>
    public class EthCoinProtocol {
        private static string GetSalt => $"ETH-SALT={DateTime.UtcNow.Millisecond}{Guid.NewGuid()}".ToMD5();

        /// <summary>
        /// 
        /// </summary>
        public static Web3 Instance = new Web3();

        /// <summary>
        ///     Try to init access to ETH API
        /// </summary>
        /// <returns></returns>
        public static bool TryInit() {
            try {
                return GenerateNewAccount(Guid.NewGuid().ToString()) != null;
            } catch {
                // API Not online
                return false;
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GenerateNewAccount(string userId) {

            // Obtain parameters for request
            var privateKey = $"{GetSalt}-{userId.ToMD5()}";

            using (var context = new ApplicationDbContext()) {

                var ethData = context.CoinSystems.First(_ => _.ShortName == "ETH");

                var web3 = new Nethereum.Web3.Web3($"http://{ethData.EndpointIP}:{ethData.EndpointPort}");
                var address = web3.Personal.NewAccount.SendRequestAsync(privateKey).Result;

                context.EthCoinPrivateKeys.Add(new EthCoinPrivateKeys {
                    AccountId = address,
                    PrivateKey = privateKey
                });

                context.SaveChanges();

                return address;
            }
        }
    }
}