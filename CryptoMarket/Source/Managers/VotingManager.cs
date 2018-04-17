using System;
using System.Data.Entity;
using System.Linq;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Quartz;

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// 
    /// </summary>
    public class VotingManager{
        /// <summary>
        /// 
        /// </summary>
        public class FindVoteJob : IJob{
            public void Execute(IJobExecutionContext jobContext){
                using (var context = new ApplicationDbContext()){
                    var votes = context.VotingForCoins.Where(vote => vote.Active).ToList();

                    var bitCoinRPC = CoinsRpcManager.Init(context.CoinSystems.First(systems => systems.ShortName == "BTC").Id.ToString());

                    foreach (var vote in votes){
                        var latestTransactions = bitCoinRPC.ListTransactions(vote.VotingAccount, 256);
                        var dbTransactions = context.VotingForCoinsTransactions.Where(voteTrans => voteTrans.VoteId == vote.Id.ToString());


                        foreach (var btcTransaction in latestTransactions.Where(btcTransaction => !dbTransactions.Any(transaction => transaction.TxId == btcTransaction.txid))){
                            // NEW VOTE FOUND     
                            vote.CurrentVotesNumber += (int) Math.Round((double) btcTransaction.amount/vote.Price, 0);
                            context.Entry(vote).State = EntityState.Modified;

                            context.VotingForCoinsTransactions.Add(new VotingForCoinsTransactions{
                                Amount = (double) btcTransaction.amount,
                                DateTime = DateTime.UtcNow,
                                TxId = btcTransaction.txid,
                                VoteId = vote.Id.ToString()
                            });

                            context.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}