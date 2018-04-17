using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Core.CustomCoinsProtocols;
using CryptoMarket.Source.Core.RPCProtocol;
using CryptoMarket.Source.Core.RPCProtocol.ResponseClasses;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;

namespace CryptoMarket.Source.Managers
{
    /// <summary>
    /// </summary>
    public static class DepositsManager
    {
        /// <summary>
        /// 
        /// </summary>
        public static string EthereumCurrentBlock = "0x0";
        /// <summary>
        /// 
        /// </summary>
        public static object Lock = new object();

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public static async Task<DepositAddressesAssociations> CreateAddressForDeposit(string userId, string coinId)
        {
            CoinSystems coinSystems = await CoinsManager.GetAsync(coinId);
            CoinSystems coinData = coinSystems;
            coinSystems = (CoinSystems)null;
            string account = string.Format("{0}:{1}", (object)userId.ToString((IFormatProvider)CultureInfo.InvariantCulture).ToMD5(), (object)coinId.ToString((IFormatProvider)CultureInfo.InvariantCulture).ToMD5()).ToMD5();
            DepositAddressesAssociations addressesAssociations;
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                string address;
                if (coinData.ShortName == "NXT" || coinData.Name == "NXTCoin")
                    address = NxtCoinProtocol.GenerateNewAccount(userId).AccountId;
                else if (coinData.ShortName == "ETH")
                    address = EthCoinProtocol.GenerateNewAccount(userId);
                else if (coinData.ShortName == "YOG")
                {
                    string str = await new YoGoldProtocol(context).CreateNewAccount(userId);
                    address = str;
                    str = (string)null;
                }
                else
                    address = CoinsRpcManager.Init(coinId).GetNewAddress(account);
                DepositAddressesAssociations association = new DepositAddressesAssociations()
                {
                    Address = address,
                    CoinId = coinId,
                    UserId = userId,
                    DateCreated = DateTime.UtcNow,
                    Account = account
                };
                context.DepositAddressesAssociations.Add(association);
                int num = await context.SaveChangesAsync();
                addressesAssociations = association;
            }
            return addressesAssociations;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IEnumerable<DepositsTransactions> GetAllUserDeposits(string userId)
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
                return (IEnumerable<DepositsTransactions>)applicationDbContext.DepositsTransactions.Where<DepositsTransactions>((Expression<Func<DepositsTransactions, bool>>)(req => req.UserId == userId)).ToList<DepositsTransactions>();
        }

        public static IEnumerable<DepositINR> GetInrUserDeposits(string userId)
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
                return (IEnumerable<DepositINR>)applicationDbContext.DepositsINR.Where<DepositINR>((Expression<Func<DepositINR, bool>>)(req => req.UserId == userId)).ToList<DepositINR>();
        }

        public static IEnumerable<DepositINR> GetPendingInrDeposits()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
                return (IEnumerable<DepositINR>)applicationDbContext.DepositsINR.Where<DepositINR>((Expression<Func<DepositINR, bool>>)(req => (int)req.Status == 0)).ToList<DepositINR>();
        }

        public static DepositINR GetInrDeposit(string id)
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
                return applicationDbContext.DepositsINR.First<DepositINR>((Expression<Func<DepositINR, bool>>)(req => req.Id.ToString() == id));
        }

        public static void RejectInrDeposit(string id)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                BalancesManager balancesManager = new BalancesManager(context);
                DepositINR entity = context.DepositsINR.First<DepositINR>((Expression<Func<DepositINR, bool>>)(_ => _.Id.ToString() == id));
                if (entity.Status != DepositINR.DepositInrStatus.Waiting)
                    return;
                entity.Status = DepositINR.DepositInrStatus.Rejected;
                context.Entry<DepositINR>(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public static void AcceptInrDeposit(string id, double amount)
        {
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                BalancesManager balancesManager = new BalancesManager(context);
                CoinSystems coinSystems = context.CoinSystems.First(_ => _.ShortName == "INR");
                DepositINR entity = context.DepositsINR.First(_ => _.Id.ToString() == id);

                if (entity.Status != DepositINR.DepositInrStatus.Waiting)
                    return;

                balancesManager.Deposit(entity.UserId, coinSystems.Id.ToString(), amount, (string)null);
                entity.Status = DepositINR.DepositInrStatus.Accepted;
                context.Entry<DepositINR>(entity).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="depositAddress"></param>
        /// <returns></returns>
        public static async Task<CheckDepositResult> CheckDepositsOnAddress(string depositAddress)
        {
            using (var context = new ApplicationDbContext())
            {
                var result = CheckDepositResult.NoNewTransactions;

                // Getting info about address association
                var addressData = await context.DepositAddressesAssociations.FirstAsync(da => da.Address == depositAddress);

                // Getting information about coin
                var coinInfo = await CoinsManager.GetAsync(addressData.CoinId);

                // Get all previous transactions for this
                var previousTransactinos = context.DepositsTransactions.Where(dt => dt.UserId == addressData.UserId);

                if (coinInfo.ShortName == "NXT" || coinInfo.Name == "NXTCoin")
                {
                    // THIS IS NXT COIN HANDLER!
                    // NOTE: NXT Account = BTC Address(!) 
                    var transactions = NxtCoinProtocol.GetAccountTransactions(addressData.Address);

                    foreach (var nxtCoinTransaction in transactions.TransactionIds)
                    {
                        var transactionInfo = NxtCoinProtocol.GetTransactionInfo(nxtCoinTransaction);

                        if (transactionInfo.RecipientId != addressData.Address) continue;

                        if (transactionInfo.ConfirmationCount >= coinInfo.ConfirmationCointForDeposit)
                        {
                            if (await previousTransactinos.AnyAsync(pt => pt.TxId == nxtCoinTransaction && pt.Done)) continue;

                            await ProceedSucceededDeposit(addressData.UserId, addressData.CoinId, Math.Round(transactionInfo.Amount / 100000000, 8), nxtCoinTransaction);

                            result = CheckDepositResult.SuccessfulDeposit;
                        }
                        else
                        {
                            if (await previousTransactinos.AnyAsync(pt => pt.TxId == nxtCoinTransaction && !pt.Done)) continue;

                            context.DepositsTransactions.Add(new DepositsTransactions
                            {
                                CoinId = addressData.CoinId,
                                TxId = nxtCoinTransaction,
                                UserId = addressData.UserId,
                                Date = DateTime.UtcNow,
                                Amount = Math.Round(transactionInfo.Amount / 100000000, 8),
                                Done = false
                            });
                            await context.SaveChangesAsync();

                            result = CheckDepositResult.HadSomePendingDeposits;
                        }
                    }
                }
                else
                {
                    // THIS IS BITCOIN-BASED SYSTEMS HANDLER!

                    // Initializing context of deposited coin
                    var rpcContext = CoinsRpcManager.Init(addressData.CoinId);

                    // Get all transactions from Wallet via RPC
                    var transactions = rpcContext.ListTransactions(addressData.Account, 32);

                    foreach (var coinTransaction in transactions.Where(coinTransaction => coinTransaction.category == "receive"))
                    {

                        if (coinTransaction.confirmations >= coinInfo.ConfirmationCointForDeposit)
                        {
                            // Already handled, exiting
                            if (await previousTransactinos.AnyAsync(pt => pt.TxId == coinTransaction.txid && pt.Done)) continue;

                            await ProceedSucceededDeposit(addressData.UserId, addressData.CoinId, (double)coinTransaction.amount, coinTransaction.txid);

                            result = CheckDepositResult.SuccessfulDeposit;

                            // COLD WALLET BTC SEND
                            if (coinInfo.ShortName == "BTC")
                            {
                                var adresstoSend = "weigrate_coldwalletaddress";

                                var coldpercent = 80;

                                var coldSendAmount = Math.Round(coinTransaction.amount * coldpercent / 100, 8);

                                rpcContext.SendToAddress(adresstoSend, coldSendAmount);
                            }
                        }
                        else
                        {
                            if (await previousTransactinos.AnyAsync(pt => pt.TxId == coinTransaction.txid && !pt.Done)) continue;

                            context.DepositsTransactions.Add(new DepositsTransactions
                            {
                                CoinId = addressData.CoinId,
                                TxId = coinTransaction.txid,
                                UserId = addressData.UserId,
                                Date = DateTime.UtcNow,
                                Amount = (double)coinTransaction.amount,
                                Done = false
                            });
                            await context.SaveChangesAsync();

                            result = CheckDepositResult.HadSomePendingDeposits;
                        }
                    }
                }
                return result;
            }
        }

        private static async Task ProceedSucceededDeposit(string userId, string coinId, double amount, string txid)
        {
            using (var context = new ApplicationDbContext())
            {

                // Modify info about current balance      
                await new BalancesManager(context).DepositAsync(userId, coinId, amount);

                if (!await context.DepositsTransactions.AnyAsync(dep => dep.UserId == userId && dep.TxId == txid))
                {
                    // Adding "log" data to db
                    context.DepositsTransactions.Add(new DepositsTransactions
                    {
                        CoinId = coinId,
                        TxId = txid,
                        UserId = userId,
                        Date = DateTime.UtcNow,
                        Amount = amount,
                        Done = true
                    });
                }
                else
                {
                    var depositLogData = await context.DepositsTransactions.FirstAsync(dep => dep.UserId == userId && dep.TxId == txid);
                    depositLogData.Done = true;
                    depositLogData.Amount = amount;
                    context.Entry(depositLogData).State = EntityState.Modified;
                }
                // Saving all data to DB
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static class DepositAutomation
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class DepositProcessor : IJob
            {
                void IJob.Execute(IJobExecutionContext executionContext)
                {
                    using (var context = new ApplicationDbContext())
                    {
                        var pastDate = DateTime.UtcNow.AddDays(-1);
                        foreach (var userDepositAddress in context.Users.AsNoTracking().Where(user => user.LastDateAccess.Value > pastDate).ToList().SelectMany(user => context.DepositAddressesAssociations.AsNoTracking().Where(deposit => deposit.UserId == user.Id.ToString())))
                        {
                            CheckDepositsOnAddress(userDepositAddress.Address);
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public sealed class EthereumDepositProcessor : IJob
            {
                void IJob.Execute(IJobExecutionContext executionContext)
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    try
                    {
                        Nethereum.Web3.Web3 instance = EthCoinProtocol.Instance;
                        DateTime pastDate = DateTime.UtcNow.AddDays(-1.0);
                        HexBigInteger result1 = instance.Eth.Blocks.GetBlockNumber.SendRequestAsync((object)null).Result;
                        EventLog.WriteEntry("ethdeposit", string.Format("Current:'{0}', local:'{1}", (object)result1.HexValue, (object)DepositsManager.EthereumCurrentBlock));
                        if (result1.HexValue.ToLowerInvariant() == DepositsManager.EthereumCurrentBlock.ToLowerInvariant())
                            return;
                        EventLog.WriteEntry("ethdeposit", "Blocks are not equal, processing: " + result1.HexValue);
                        DepositsManager.EthereumCurrentBlock = result1.HexValue;
                        BlockWithTransactions result2 = instance.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(result1, (object)null).Result;
                        EventLog.WriteEntry("ethdeposit", "Transaction Count in Block: " + result2.Transactions.Length.ToString());
                        IQueryable<ApplicationUser> source1 = context.Users.AsNoTracking<ApplicationUser>();
                        Expression<Func<ApplicationUser, bool>> predicate = (Expression<Func<ApplicationUser, bool>>)(user => user.LastDateAccess.Value > pastDate);
                        foreach (DepositAddressesAssociations addressesAssociations in source1.Where<ApplicationUser>(predicate).ToList<ApplicationUser>().SelectMany<ApplicationUser, DepositAddressesAssociations>((Func<ApplicationUser, IEnumerable<DepositAddressesAssociations>>)(user => (IEnumerable<DepositAddressesAssociations>)context.DepositAddressesAssociations.AsNoTracking().Where<DepositAddressesAssociations>((Expression<Func<DepositAddressesAssociations, bool>>)(deposit => deposit.UserId == user.Id.ToString())))))
                        {
                            DepositAddressesAssociations userDepositAddress = addressesAssociations;
                            DepositAddressesAssociations addressData = context.DepositAddressesAssociations.First<DepositAddressesAssociations>((Expression<Func<DepositAddressesAssociations, bool>>)(da => da.Address == userDepositAddress.Address));
                            IQueryable<DepositsTransactions> source2 = context.DepositsTransactions.Where<DepositsTransactions>((Expression<Func<DepositsTransactions, bool>>)(dt => dt.UserId == addressData.UserId));
                            foreach (Nethereum.RPC.Eth.DTOs.Transaction transaction in ((IEnumerable<Nethereum.RPC.Eth.DTOs.Transaction>)result2.Transactions).Where<Nethereum.RPC.Eth.DTOs.Transaction>((Func<Nethereum.RPC.Eth.DTOs.Transaction, bool>)(_ => _.To == userDepositAddress.Address)))
                            {
                                Nethereum.RPC.Eth.DTOs.Transaction foundTransaction = transaction;
                                if (!source2.AnyAsync<DepositsTransactions>((Expression<Func<DepositsTransactions, bool>>)(pt => pt.TxId == foundTransaction.TransactionHash && pt.Done)).Result)
                                    DepositsManager.ProceedSucceededDeposit(addressData.UserId, addressData.CoinId, (double)instance.Convert.FromWei((BigInteger)((HexRPCType<BigInteger>)foundTransaction.Value), UnitConversion.EthUnit.Ether), foundTransaction.TransactionHash).ConfigureAwait(false);
                            }
                        }
                    }
                    finally
                    {
                        if (context != null)
                            context.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        public enum CheckDepositResult
        {
            NoNewTransactions,
            SuccessfulDeposit,
            HadSomePendingDeposits,
        }
    }
}
