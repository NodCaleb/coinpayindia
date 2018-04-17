#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// 
    /// </summary>
    public static class AccountingManager{
        /// <summary>
        /// 
        /// </summary>
        public enum AccountingLogType{
            /// <summary>
            /// 
            /// </summary>
            FeeAdding,
            /// <summary>
            /// 
            /// </summary>
            FeeWithdrawing
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AccountingFees>> GetAvailableFeesAsync(){
            using (var context = new ApplicationDbContext()){
                return await context.AccountingFees.ToListAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static async Task AddWithdrawableFeeAsync(string coinId, double amount){
            using (var context = new ApplicationDbContext()){
                // Check existance
                var isAnyFeesForThisCoin = await context.AccountingFees.AnyAsync(coin => coin.CoinId == coinId);
                if (!isAnyFeesForThisCoin){
                    context.AccountingFees.Add(new AccountingFees{
                        AvailableAmount = 0,
                        CoinId = coinId
                    });
                    // Save
                    await context.SaveChangesAsync();
                }

                // Logging
                await AddLogAsync(AccountingLogType.FeeAdding, string.Format("{0}: {1} Fee Added", CoinsManager.GetCoinNameById(coinId), amount));

                // Getting fee data
                var feeData = await context.AccountingFees.FirstAsync(coin => coin.CoinId == coinId);
                // Setting fee
                feeData.AvailableAmount += amount;
                // Db state
                context.Entry(feeData).State = EntityState.Modified;
                // Save
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task WithdrawFeeToAdminAddressAsync(string coinId, double amount){
            using (var context = new ApplicationDbContext()){
                var coinSystemInfo = await context.CoinSystems.FirstAsync(coin => coin.Id.ToString() == coinId);
                // Getting fee data
                var feeData = await context.AccountingFees.FirstAsync(coin => coin.CoinId == coinId);
                // Check amount
                if (feeData.AvailableAmount - amount < 0){
                    throw new Exception("Amount greater than available");
                }

                feeData.AvailableAmount -= amount;

                context.Entry(feeData).State = EntityState.Modified;

                // Logging
                await AddLogAsync(AccountingLogType.FeeWithdrawing, string.Format("{0}: {1} Fee Withdraw to {2}", CoinsManager.GetCoinNameById(coinId), amount, coinSystemInfo.AdminWallet));

                // Initializing the RPC
                var rpcContext = await CoinsRpcManager.InitAsync(coinSystemInfo.Id.ToString());
                // Send to admin
                rpcContext.SendToAddress(coinSystemInfo.AdminWallet, (decimal) amount, "Admin Withdraw");
                // Save
                await context.SaveChangesAsync();
            }
        }

        private static async Task AddLogAsync(AccountingLogType type, string text){
            using (var context = new ApplicationDbContext()){
                context.AccountingLogs.Add(new AccountingLogs{
                    DateTime = DateTime.UtcNow,
                    Text = text,
                    Type = type
                });

                await context.SaveChangesAsync();
            }
        }
    }
}