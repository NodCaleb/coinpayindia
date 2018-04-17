#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Quartz;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// </summary>
    public static class WithdrawManager{
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static async Task<List<WithdrawRequests>> GetAllNonAuto(){
            using (var context = new ApplicationDbContext()){
                return await context.WithdrawRequests.Where(req => !req.Paid && !req.Auto).ToListAsync();
            }
        }

        public static List<WithdrawRequests> GetAllUserWithdraws(string userId){
            using (var context = new ApplicationDbContext()){
                return context.WithdrawRequests.Where(req => req.UserId == userId).ToList();
            }
        }

        public static List<INRWithdrawRequest> GetPendingInrWithdrawals()
        {
            using (var context = new ApplicationDbContext())
            {
                return context.INRWithdrawRequests.Where(req => req.Executed == false).ToList();
            }
        }

        public static void InrWithdrawComplete(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                var withdraw = context.INRWithdrawRequests.First(req => req.Id.ToString() == id);

                withdraw.Executed = true;
                withdraw.ExecutionDateTime = DateTime.UtcNow;

                context.Entry(withdraw).State = EntityState.Modified;

                context.SaveChanges();
            }
        }

        public static INRWithdrawRequest GetInrWithdrawById(string id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.INRWithdrawRequests.First(req => req.Id.ToString() == id);
            }
        }

        public static List<INRWithdrawRequest> GetAllUserInrWithdraws(string userId)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.INRWithdrawRequests.Where(req => req.UserId == userId).ToList();
            }
        }

        public static void AddInrWithdraw(string userId, string customerName, string customerAccountNumber, string bankName, string IFSCcode, double amount)
        {
            using (var context = new ApplicationDbContext())
            {
                context.INRWithdrawRequests.Add(new INRWithdrawRequest
                {
                    UserId = userId,
                    CustomerName = customerName,
                    CustomerAccountNumber = customerAccountNumber,
                    BankName = bankName,
                    Amount = amount,
                    IFSCCode = IFSCcode,
                    Executed = false,
                    CreationDateTime = DateTime.UtcNow
                });

                context.SaveChanges();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static async Task<List<WithdrawRequests>> GetAllDone(){
            using (var context = new ApplicationDbContext()){
                return await context.WithdrawRequests.Where(req => req.Paid).ToListAsync();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static int GetPendingWithdrawsCount(){
            using (var context = new ApplicationDbContext()){
                return context.WithdrawRequests.Count(req => !req.Paid && !req.Auto);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task Accept(string id){
            using (var context = new ApplicationDbContext()){
                // Getting withdrawal information
                var withdrawRequestData = await context.WithdrawRequests.FirstAsync(req => req.Id.ToString() == id);
                // Setting withdraw to "AUTO" so withdraw engine can withdraw it
                withdrawRequestData.Auto = true;
                // Setting "modified"
                context.Entry(withdrawRequestData).State = EntityState.Modified;
                // Save
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task Decline(string id){
            using (var context = new ApplicationDbContext()){
                var withdrawRequestData = await context.WithdrawRequests.FirstAsync(req => req.Id.ToString() == id);
                // Delete from database
                context.Entry(withdrawRequestData).State = EntityState.Deleted;
                // Return money to user
                await new BalancesManager(context).DepositAsync(withdrawRequestData.UserId, withdrawRequestData.CoinId, withdrawRequestData.Amount);
                // Save
                await context.SaveChangesAsync();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public class WithdrawProcessor : IJob{
            void IJob.Execute(IJobExecutionContext executionContext){
                using (var context = new ApplicationDbContext()){
                    foreach (var withdrawRequests in context.WithdrawRequests.Where(req => !req.Paid || req.TxId == "pending...").ToList()){
                        try{
                            var rpcInit = CoinsRpcManager.Init(withdrawRequests.CoinId);


                            var txId = rpcInit.SendToAddress(withdrawRequests.Address, (decimal)Math.Round(withdrawRequests.Amount,8));

                            if (txId.Length > 32)
                            {
                                withdrawRequests.TxId = txId;
                                withdrawRequests.Paid = true;
                                withdrawRequests.DatePaid = DateTime.UtcNow;
                                context.Entry(withdrawRequests).State = EntityState.Modified;

                                context.SaveChanges();
                            }

                            // Double check
                            

                        }
                        catch{
                            // DO Nothing
                        } 
                    }
                }
            }
        }
    }
}