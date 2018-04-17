#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// </summary>
    public class BalancesManager{
        /// <summary>
        /// 
        /// </summary>
        private ApplicationDbContext Context { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public BalancesManager(ApplicationDbContext context){
            Context = context;
        }

        /// <summary>
        /// </summary>
        public enum BalanceUpdateType{
            /// <summary>
            /// 
            /// </summary>
            Deposit,

            /// <summary>
            /// 
            /// </summary>
            Withdraw
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IEnumerable<Balances> GetAllBalances(string userId){
            using (var context = new ApplicationDbContext()){
                return context.Balances.Where(bal => bal.UserId == userId).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public static Balances Get(string userId, string coinId){
            using (var context = new ApplicationDbContext()){
                if (context.Balances.Any(balance => balance.UserId == userId && balance.CoinId == coinId)){
                    return context.Balances.First(balance => balance.UserId == userId && balance.CoinId == coinId);
                }

                var balanceData = new Balances{
                    CoinId = coinId,
                    UserId = userId,
                    Balance = 0.0
                };

                context.Balances.Add(balanceData);

                context.SaveChanges();

                return balanceData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="coinId"></param>
        /// <param name="value"></param>
        /// <param name="note"></param>
        public void Deposit(string userId, string coinId, double value, string note = null){
            var balanceData = Context.Balances.First(balance => balance.UserId == userId && balance.CoinId == coinId);

            balanceData.Balance += value;
            Context.Entry(balanceData).State = EntityState.Modified;

            try {
                SingalRManager.BalanceUpdateSendToClient(userId, coinId, balanceData.Balance, value, BalanceUpdateType.Deposit);
            } catch {
                // ignored
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="coinId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task DepositAsync(string userId, string coinId, double value) {
            var balanceData = await Context.Balances.FirstAsync(balance => balance.UserId == userId && balance.CoinId == coinId);

            balanceData.Balance += value;
            Context.Entry(balanceData).State = EntityState.Modified;

            try {
                SingalRManager.BalanceUpdateSendToClient(userId, coinId, balanceData.Balance, value, BalanceUpdateType.Deposit);
            } catch {
                // ignored
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="coinId"></param>
        /// <param name="value"></param>
        /// <exception cref="Exception">[Withdraw] Hack Protection. Use only positive numbers.</exception>
        public void Withdraw(string userId, string coinId, double value){
            var balanceData = Context.Balances.First(balance => balance.UserId == userId && balance.CoinId == coinId);

            if (value <= 0){
                throw new Exception("[Withdraw] Hack Protection. Use only positive numbers.");
            }

            balanceData.Balance -= value;

            if(balanceData.Balance < 0){
                throw new Exception("[Withdraw] Zero Issue");
            }

            Context.Entry(balanceData).State = EntityState.Modified;

            try { 
            SingalRManager.BalanceUpdateSendToClient(userId, coinId, balanceData.Balance, value, BalanceUpdateType.Withdraw);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        public void InitiateBalancesAndValidateNew(string userId){
            var allCoins = Context.CoinSystems.ToList();
            var allUserCoins = Context.Balances.Where(usr => usr.UserId == userId);

            foreach (var coin in allCoins.Where(coin => !allUserCoins.Any(user => user.CoinId == coin.Id.ToString()))){
                Context.Balances.Add(new Balances{
                    CoinId = coin.Id.ToString(),
                    UserId = userId,
                    Balance = 0.0
                });
            }

            Context.SaveChanges();
        }

        public void InitiateFakeBalancesAndValidateNew(string userId) {
            var allCoins = Context.CoinSystems.ToList();
            var allUserCoins = Context.Balances.Where(usr => usr.UserId == userId);

            foreach (var coin in allCoins.Where(coin => !allUserCoins.Any(user => user.CoinId == coin.Id.ToString()))) {
                Context.Balances.Add(new Balances {
                    CoinId = coin.Id.ToString(),
                    UserId = userId,
                    Balance = 100
                });
            }

            Context.SaveChanges();
        }
    }
}