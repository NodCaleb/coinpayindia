#region

using System;
using System.Data.Entity;
using System.Linq;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Managers;
using Quartz;

#endregion

namespace CryptoMarket.Source{
    /// <summary>
    /// 
    /// </summary>
    public class MatchingEngine{
        /// <summary>
        ///     Thread-safe stats comparsion.
        /// </summary>
        private static readonly object Lock = new object();

        /// <summary>
        ///     Auto-executing job(every 15 seconds)
        ///     Fucking hate this part. Really. Don't recommend anybody to watch code below..
        /// </summary>
        public class MatchJob : IJob{

            private class OrderProcessor{
                private readonly ApplicationDbContext _context;

                public OrderProcessor(ApplicationDbContext context){
                    _context = context;
                }

                public void CloseOrder(Order order, string closerOrderId, string closerUserId){
                    order.Closed = true;
                    order.DateClosed = DateTime.UtcNow;
                    order.ClosingOrderId = closerOrderId;
                    order.CloserUserId = closerUserId;
                    order.PartialOrderTotalLeft = 0;

                    _context.Entry(order).State = EntityState.Modified;
                }
            }

            public void Execute(IJobExecutionContext jobContext){
                // Lock code parallel execute to better stats accuracy
                lock (Lock){
                    using (var context = new ApplicationDbContext()){

                        var balancesManager = new BalancesManager(context);
                        var orderProcessor = new OrderProcessor(context);
                        var allOrders = context.Orders.Where(order => order.Closed == false).ToList();

                        // For each market, proceed matching
                        foreach (var market in context.Markets.ToList()){
                            // All market orders
                            var orders = allOrders.Where(order => order.MarketId == market.Id.ToString()).ToList();

                            // WE WORK FROM "SELL". Pairing process -> SELL to BUY
                            // Processing MATCHING ORDERS
                            foreach (var sellOrder in orders.OrderBy(order => order.Price).Where(order => order.TradeType == TradeType.Sell)){

                                var buyOrdersThatNotMatchedButProcess = orders.Where(order => order.Price > sellOrder.Price && order.TradeType == TradeType.Buy && !order.Closed).OrderBy(orderby => orderby.Price).ToList();
                                foreach (var buyOrder in buyOrdersThatNotMatchedButProcess){
                                    if (buyOrder.Closed || sellOrder.Closed) continue;

                                    if (buyOrder.Price > sellOrder.Price){
                                        if (sellOrder.PartialOrderTotalLeft >= buyOrder.PartialOrderTotalLeft){

                                            var withdrawingFee = (buyOrder.PartialOrderTotalLeft * market.Fee / 100);
   
                                            // This value = (Net Total - Total) / Total Left == Order Fee for One Unit
                                            var sellOrderFeeForOne = Math.Round((sellOrder.NetTotal - sellOrder.Total)/sellOrder.PartialOrderTotalLeft, 8);

                                            // Total Fee of Sell order, based on Buy Order Amounts 
                                            var actualFee = Math.Round(sellOrderFeeForOne*buyOrder.Amount, 8);

                                            // Get Total Fee = Buy Order - Actual Fee
                                            var savingsFee = Math.Round(buyOrder.FeeTotal - actualFee, 8);

                                            // Save amount = Buy Price - Sell Price * Buy Amount
                                            var savingsPrice = Math.Round((buyOrder.Price - sellOrder.Price)*buyOrder.Amount, 8);

                                            var savings = Math.Round(savingsFee + savingsPrice, 8);

                                            // Deposit Buyer his Savings
                                            balancesManager.Deposit(buyOrder.UserId, market.CoinFrom, savings);
                                            // Deposit Buyer his Order amount (Full Partial, because we process Sell Amount > Buy Amount)
                                            balancesManager.Deposit(buyOrder.UserId, market.CoinTo, buyOrder.PartialOrderTotalLeft - withdrawingFee);
                                            // Deposit Seller his Order Amount

                                            var sellerAmount = Math.Round(buyOrder.Total - (buyOrder.Total * market.Fee / 100), 8);
                                            balancesManager.Deposit(sellOrder.UserId, market.CoinFrom, buyOrder.Amount);    

                                            // Setting amount, that left from to Sell Order, because we close Buy Order fully 
                                            sellOrder.PartialOrderTotalLeft = Math.Round(sellOrder.PartialOrderTotalLeft - buyOrder.PartialOrderTotalLeft, 8);
                                            // Re-calculate Total, FeeTotal and NetTotal
                                            sellOrder.Total = sellOrder.PartialOrderTotalLeft*sellOrder.Price;
                                            sellOrder.FeeTotal = sellOrder.Total*market.Fee/100;
                                            sellOrder.NetTotal = Math.Round(sellOrder.Total + sellOrder.FeeTotal, 8);

                                            if (sellOrder.PartialOrderTotalLeft == 0){
                                                orderProcessor.CloseOrder(sellOrder, buyOrder.Id.ToString(), buyOrder.UserId);
                                            }

                                            orderProcessor.CloseOrder(buyOrder, sellOrder.Id.ToString(), sellOrder.UserId);      

                                            SingalRManager.OrderClosedSendToClient(buyOrder.UserId, $"[P:3]Order #{buyOrder.Id} proceeed at {DateTime.UtcNow}.", market.Id.ToString());
                                            SingalRManager.OrderClosedSendToClient(sellOrder.UserId, $"[P:3]Order #{sellOrder.Id} proceeed at {DateTime.UtcNow}. ", market.Id.ToString());

                                            context.SaveChanges();

                                            AccountingManager.AddWithdrawableFeeAsync(market.CoinTo, buyOrder.FeeTotal).ConfigureAwait(false); ;

                                            GraphStatsManager.Write(sellOrder.MarketId, buyOrder.Price, sellOrder.Total);

                                            SingalRManager.NewSellBuyOrdersSendToAll(market.Id.ToString());
                                        }

                                        if (sellOrder.PartialOrderTotalLeft > buyOrder.PartialOrderTotalLeft){

                                            // This value = (Net Total - Total) / Total Left == Order Fee for One Unit
                                            var buyOrderFeeForOne = Math.Round((buyOrder.NetTotal - buyOrder.Total) / buyOrder.PartialOrderTotalLeft, 8);

                                            // Total Fee of Sell order, based on Buy Order Amounts 
                                            var actualFee = Math.Round(buyOrderFeeForOne * sellOrder.Amount, 8);

                                            // Get Total Fee = Buy Order - Actual Fee
                                            var savingsFee = Math.Round(actualFee - sellOrder.FeeTotal, 8);

                                            // Save amount = Buy Price - Sell Price * Buy Amount
                                            var savingsPrice = Math.Round((buyOrder.Price - sellOrder.Price) * sellOrder.Amount, 8);

                                            // Deposit Buyer his Savings
                                            balancesManager.Deposit(buyOrder.UserId, market.CoinFrom, Math.Round(savingsFee + savingsPrice, 8));
                                            // Deposit Buyer his Order amount (Full Partial, because we process Sell Amount > Buy Amount)
                                            balancesManager.Deposit(buyOrder.UserId, market.CoinTo, Math.Round(sellOrder.PartialOrderTotalLeft - (sellOrder.PartialOrderTotalLeft*market.Fee/100), 8));

                                            // Deposit Seller his Order Amount
                                            balancesManager.Deposit(sellOrder.UserId, market.CoinFrom, Math.Round(sellOrder.Total - (sellOrder.Total*market.Fee/100), 8));

                                            // Setting amount, that left from to Buy Order, because we close Sell Order fully 
                                            buyOrder.PartialOrderTotalLeft = Math.Round(buyOrder.PartialOrderTotalLeft - sellOrder.PartialOrderTotalLeft, 8);
                                            // Re-calculate Total, FeeTotal and NetTotal
                                            buyOrder.Total = buyOrder.PartialOrderTotalLeft * buyOrder.Price;
                                            buyOrder.FeeTotal = buyOrder.Total * market.Fee / 100;
                                            buyOrder.NetTotal = Math.Round(buyOrder.Total + buyOrder.FeeTotal, 8);

                                            if (sellOrder.PartialOrderTotalLeft == 0) {
                                                orderProcessor.CloseOrder(buyOrder, sellOrder.Id.ToString(), sellOrder.UserId);
                                            }

                                            orderProcessor.CloseOrder(sellOrder, buyOrder.Id.ToString(), buyOrder.UserId);

                                            SingalRManager.OrderClosedSendToClient(buyOrder.UserId,
                                                $"[PROC:4]Order #{buyOrder.Id} proceeed at {DateTime.UtcNow}. Thanks for using our service.", market.Id.ToString());
                                            SingalRManager.OrderClosedSendToClient(sellOrder.UserId,
                                                $"[PROC:4]Order #{sellOrder.Id} proceeed at {DateTime.UtcNow}. Thanks for using our service.", market.Id.ToString());

                                            context.SaveChanges();

                                            AccountingManager.AddWithdrawableFeeAsync(market.CoinTo, buyOrder.FeeTotal).ConfigureAwait(false); ;
                                            GraphStatsManager.Write(sellOrder.MarketId, buyOrder.Price, sellOrder.Total);

                                            SingalRManager.NewSellBuyOrdersSendToAll(market.Id.ToString());
                                        }
                                    }
                                }

                                // If any matching orders, proceed next        
                                var matchedBuyOrders = orders.Where(ordr => ordr.TradeType == TradeType.Buy && ordr.Price == sellOrder.Price).ToList();
                                foreach (var matchedBuyOrder in matchedBuyOrders){
                                    if (matchedBuyOrder.Closed || sellOrder.Closed) continue;

                                    // IF FIRST BUY ORDER BIGGER THAN SELL, FULL-PROCESSING!
                                    if (matchedBuyOrder.PartialOrderTotalLeft >= sellOrder.PartialOrderTotalLeft){    
                                        var withdrawingFee = (sellOrder.PartialOrderTotalLeft * market.Fee/100);

                                        // Selling user info modification:     
                                        var sellerNewBalance = sellOrder.PartialOrderTotalLeft - (sellOrder.PartialOrderTotalLeft * market.Fee / 100);
                                        balancesManager.Deposit(matchedBuyOrder.UserId, market.CoinTo, sellerNewBalance);

                                        // Buying user info modification:     
                                        balancesManager.Deposit(sellOrder.UserId, market.CoinFrom, sellOrder.Total - withdrawingFee);

                                        matchedBuyOrder.PartialOrderTotalLeft = Math.Round(matchedBuyOrder.PartialOrderTotalLeft - sellOrder.PartialOrderTotalLeft, 8);
                                        matchedBuyOrder.Total = matchedBuyOrder.PartialOrderTotalLeft*matchedBuyOrder.Price;
                                        matchedBuyOrder.FeeTotal = matchedBuyOrder.Total*market.Fee/100;
                                        matchedBuyOrder.NetTotal = Math.Round(matchedBuyOrder.Total + matchedBuyOrder.FeeTotal, 8);

                                        if (matchedBuyOrder.PartialOrderTotalLeft == 0){
                                            orderProcessor.CloseOrder(matchedBuyOrder, sellOrder.Id.ToString(), sellOrder.UserId);
                                        }

                                        orderProcessor.CloseOrder(sellOrder, matchedBuyOrder.Id.ToString(), matchedBuyOrder.UserId);

                                        context.SaveChanges();

                                        SingalRManager.OrderClosedSendToClient(matchedBuyOrder.UserId, $"[P:1]Order #{matchedBuyOrder.Id} proceeed at {DateTime.UtcNow}.", market.Id.ToString());
                                        SingalRManager.OrderClosedSendToClient(sellOrder.UserId, $"[P:1]Order #{sellOrder.Id} proceeed at {DateTime.UtcNow}.", market.Id.ToString());

                                        AccountingManager.AddWithdrawableFeeAsync(market.CoinFrom, withdrawingFee).ConfigureAwait(false);

                                        GraphStatsManager.Write(matchedBuyOrder.MarketId, matchedBuyOrder.Price, matchedBuyOrder.Total);

                                        SingalRManager.NewSellBuyOrdersSendToAll(market.Id.ToString());
                                    }

                                    if (sellOrder.PartialOrderTotalLeft > matchedBuyOrder.PartialOrderTotalLeft){
                                        var withdrawingFee = (matchedBuyOrder.Amount*market.Fee/100);

                                        // Selling user info modification:
                                        balancesManager.Deposit(matchedBuyOrder.UserId, market.CoinTo, matchedBuyOrder.PartialOrderTotalLeft - (matchedBuyOrder.PartialOrderTotalLeft * market.Fee / 100));

                                        // Buying user info modification:
                                        balancesManager.Deposit(sellOrder.UserId, market.CoinFrom, matchedBuyOrder.Total - withdrawingFee);

                                        sellOrder.PartialOrderTotalLeft = Math.Round(sellOrder.PartialOrderTotalLeft - matchedBuyOrder.PartialOrderTotalLeft, 8);
                                        sellOrder.Total = sellOrder.PartialOrderTotalLeft*sellOrder.Price;
                                        sellOrder.FeeTotal = sellOrder.Total*market.Fee/100;
                                        sellOrder.NetTotal = Math.Round(sellOrder.Total + sellOrder.FeeTotal, 8);

                                        if (sellOrder.PartialOrderTotalLeft == 0){
                                            orderProcessor.CloseOrder(sellOrder, matchedBuyOrder.Id.ToString(), matchedBuyOrder.UserId);
                                        }

                                        orderProcessor.CloseOrder(matchedBuyOrder, sellOrder.Id.ToString(), sellOrder.UserId);

                                        context.SaveChanges();

                                        SingalRManager.OrderClosedSendToClient(matchedBuyOrder.UserId,
                                            $"[P:2]Order #{matchedBuyOrder.Id} proceeed at {DateTime.UtcNow}.", market.Id.ToString());
                                        SingalRManager.OrderClosedSendToClient(sellOrder.UserId,
                                            $"[P:2]Order #{sellOrder.Id} proceeed at {DateTime.UtcNow}.", market.Id.ToString());

                                        AccountingManager.AddWithdrawableFeeAsync(market.CoinFrom, withdrawingFee).ConfigureAwait(false);

                                        GraphStatsManager.Write(matchedBuyOrder.MarketId, matchedBuyOrder.Price, sellOrder.Total);

                                        SingalRManager.NewSellBuyOrdersSendToAll(market.Id.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}