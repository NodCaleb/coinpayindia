#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Core;
using CryptoMarket.Source.Managers;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using RestSharp.Extensions;

#endregion

namespace CryptoMarket.Controllers {
    /// <summary>
    /// Market Page Logic
    /// </summary>
    [RequireHttps]

    public class MarketController : AsyncController {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        /// <summary>
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        public async Task<ViewResult> Index(string pair) {

            var market = new Markets();

            if (pair.HasValue()) {
                if (_db.Markets.Any(_ => _.PairName == pair)) {
                    market = _db.Markets.First(_ => _.PairName == pair);
                }
                else
                {
                    market = _db.Markets.First(_ => _.PairName == "YOC/INR");
                }
            } else {
                market = _db.Markets.First(_ => _.PairName == "YOC/INR");
            }
                
            var data = MarketsManager.GetStructuredOrdersData(market.Id.ToString());

            ViewBag.BuyOrders = data.BuyOrders;
            ViewBag.SellOrders = data.SellOrders;
            ViewBag.DayHigh = data.DayHigh;
            ViewBag.DayLow = data.DayLow;
            return View(market);
        }

        public ActionResult Support()
        {
            return View();
        }

        /// <summary>
        /// GET: Fees
        /// </summary>
        public ActionResult Fees() {
            var returnData = new HomeViewModels.FeesPageModel {
                TradingFees = new Dictionary<string, string>(),
                WithdrawFees = new Dictionary<string, string>()
            };
            using (var db = new ApplicationDbContext()) {
                foreach (var market in db.Markets.ToList()) {
                    returnData.TradingFees.Add(market.PairName, market.Fee.ToString("F4"));
                }

                foreach (var coin in db.CoinSystems.DistinctBy(coin => coin.Name).ToList()) {
                    returnData.WithdrawFees.Add(coin.Name, coin.WithdrawalFee.ToString("F8"));
                }
            }
            return View(returnData);
        }

        /// <summary>
        /// GET: Voting
        /// </summary>
        public async Task<ViewResult> Voting() {
            using (var db = new ApplicationDbContext()) {
                return View(await db.VotingForCoins.Where(vote => vote.Active).ToListAsync());
            }
        }

        /// <summary>
        /// GET: Voting
        /// </summary>
        public ViewResult Faq() {
            return View();
        }

        /// <summary>
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public JsonResult GetBuySellOrders(string marketId) {
            return Json(MarketsManager.GetStructuredOrdersData(marketId), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public JsonResult GetUserActiveOrders(string marketId) {
            return Json(OrdersManager.GetUserActiveOrdersByMarketIdAsync(marketId, User.Identity.GetUserId()), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public void ChatMessage(string message) {
            ChatManager.AddChatMessage(User.Identity.GetUserId(), message);
        }


        /// <summary>
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public JsonResult DeleteOrder(string orderId) {
            return Json(OrdersManager.Delete(orderId, User.Identity.GetUserId()), JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing) {
            if (disposing) {
                _db.Dispose();
            }
            base.Dispose(disposing);
            _db.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        public class BuyRequestModel {
            /// <summary>
            /// 
            /// </summary>
            public double BuyAmount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double BuyPricePerOne { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        public class BuySellRequestResponse {
            /// <summary>
            /// 
            /// </summary>
            public enum BuySellRequestStatus {
                /// <summary>
                /// 
                /// </summary>
                Success,

                /// <summary>
                /// 
                /// </summary>
                Error
            }

            /// <summary>
            /// 
            /// </summary>
            public BuySellRequestStatus Status { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string Error { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public class SellRequestModel {
            /// <summary>
            /// 
            /// </summary>
            public double SellAmount { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double SellPricePerOne { get; set; }
        }

    }
}