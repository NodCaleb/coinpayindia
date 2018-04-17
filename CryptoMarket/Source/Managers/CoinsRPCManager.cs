#region

using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Source.Core.RPCProtocol;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// 
    /// </summary>
    public static class CoinsRpcManager{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public static BitcoinRPC Init(string coinId){
            using (var context = new ApplicationDbContext()){
                var coinRpcData = context.CoinSystems.First(coin => coin.Id.ToString() == coinId);
                return new BitcoinRPC(new Uri(string.Format("http://{0}:{1}", coinRpcData.EndpointIP, coinRpcData.EndpointPort)), new NetworkCredential(coinRpcData.EndpointLogin, coinRpcData.EndpointPassword));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public static async Task<BitcoinRPC> InitAsync(string coinId){
            using (var context = new ApplicationDbContext()){
                var coinRpcData = await context.CoinSystems.FirstAsync(coin => coin.Id.ToString() == coinId);
                return new BitcoinRPC(new Uri(string.Format("http://{0}:{1}", coinRpcData.EndpointIP, coinRpcData.EndpointPort)), new NetworkCredential(coinRpcData.EndpointLogin, coinRpcData.EndpointPassword));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="login"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        public static async Task<bool> CheckAvailability(string ip, int port, string login, string pw){
            try{
                var rpc = new BitcoinRPC(new Uri(string.Format("http://{0}:{1}", ip, port)), new NetworkCredential(login, pw));
                return rpc.ValidateAddress(rpc.GetNewAddress("InitTesing")).isvalid;
            }
            catch{
                return false;
            }
        }
    }
}