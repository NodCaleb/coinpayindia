#region

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// 
    /// </summary>
    public static class CoinsManager{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public static string GetCoinNameById(string coinId){
            using (var context = new ApplicationDbContext()){
                if(context.CoinSystems.Any(coin => coin.Id.ToString() == coinId))
                    return context.CoinSystems.First(coin => coin.Id.ToString() == coinId).Name;
                else 
                    return "Check Market Name";               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public static string GetCoinShortNameById(string coinId){
            using (var context = new ApplicationDbContext()){
                return context.CoinSystems.First(coin => coin.Id.ToString() == coinId).ShortName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public static double GetCoinMinimumTradeAmounteById(string coinId) {
            using (var context = new ApplicationDbContext()) {
                return context.CoinSystems.First(coin => coin.Id.ToString() == coinId).MinimumTradeAmount;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        public async static Task ToggleWithdraw(string coinId){
            using (var context = new ApplicationDbContext()){
                var coinInfo = await context.CoinSystems.FirstAsync(coin => coin.Id.ToString() == coinId);

                coinInfo.WithdrawDisabled = !coinInfo.WithdrawDisabled;

                context.Entry(coinInfo).State = EntityState.Modified;

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        public static async Task ToggleDeposit(string coinId){
            using (var context = new ApplicationDbContext()){
                var coinInfo = await context.CoinSystems.FirstAsync(coin => coin.Id.ToString() == coinId);

                coinInfo.DepositDisabled = !coinInfo.DepositDisabled;

                context.Entry(coinInfo).State = EntityState.Modified;

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CoinSystems> GetAsync(string id){
            using (var context = new ApplicationDbContext()){
                return await context.CoinSystems.FirstAsync(coin => coin.Id.ToString() == id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CoinSystems Get(string id){
            using (var context = new ApplicationDbContext()){
                return context.CoinSystems.First(coin => coin.Id.ToString() == id);
            }
        }

        public static CoinSystems GetByShortName(string shortName)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.CoinSystems.First(coin => coin.ShortName == shortName);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CoinSystems>> GetAllAsync(){
            using (var context = new ApplicationDbContext()){
                return await context.CoinSystems.ToListAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static async Task EditAsync(CoinSystems model){
            using (var context = new ApplicationDbContext()){
                var coinCurrentData = await GetAsync(model.Id.ToString());

                model.AdminWallet = coinCurrentData.AdminWallet;

                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }
    }
}