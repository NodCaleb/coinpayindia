using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using BtcE;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Core;
using CryptoMarket.Source.Core.Platforms.BTC_e;
using CryptoMarket.Source.Core.Platforms.Database;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;

namespace CryptoMarket.Controllers{
    /// <summary>
    /// 
    /// </summary>
    [System.Web.Mvc.Authorize]
    [RequireHttps]
    public class PlatformsController : Controller{


        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        public PlatformsController(){
            _context = new ApplicationDbContext();
        }

         ~PlatformsController(){
            Dispose(false);
            _context.Dispose();         
        }

        // GET: Platforms
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(){
            var userId = User.Identity.GetUserId();
            var userData = _context.Users.First(user => user.Id.ToString() == userId);
            ViewBag.BtceActive = !userData.BtceKey.IsNullOrWhiteSpace();

            return View();

        }

        public class BtceTradePageModel{
            public BtcePair Pair { get; set; }
            public Ticker Ticker { get; set; }
            public List<BtceOrder> OrderList { get; set; }
            public OrderTypes OrderType { get; set; }
            public decimal StopProfitPrice { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        public ActionResult Btce(string pair){
            BtcePair parseResult;
            Enum.TryParse(pair, true, out parseResult);

            if (parseResult == BtcePair.Unknown)
                parseResult = BtcePair.btc_usd;

            var userId = User.Identity.GetUserId();

            var apiCredentials = APIKeysManager.GetApiCredentials(userId);

            var btceClientApi = new BtceApiClientV3(apiCredentials.Key, apiCredentials.Secret);

            var orders = new OrderList();
            try{
                orders = btceClientApi.GetActiveOrders(parseResult);
            }
            catch{
                // ignored
            }

            var tradeHistory = new TradeHistory();
            try{
                tradeHistory = btceClientApi.GetTradeHistory(pair: parseResult);
            }
            catch{
                // ignored
            }

            var model = new BtceTradePageModel{
                Pair = parseResult,
                Ticker = BtceApi.GetTicker(parseResult),
                OrderList = _context.BtceOrders.Where(_=>_.UserId == userId).ToList()
                   

            };

            return View(model);
        }

        public class TradeBtceRequestModel{
            public decimal Amount { get; set; }
            public decimal Price { get; set; }
            public OrderTypes OrderType { get; set; }

            public decimal StopProfitPrice { get; set; }

        }

        [System.Web.Mvc.HttpPost, ValidateAntiForgeryToken]
        public ActionResult TradeBtce([FromUri] BtcePair pair, [FromUri] TradeType type, [FromBody] TradeBtceRequestModel model){
            var userId = User.Identity.GetUserId();
            var apiCredentials = APIKeysManager.GetApiCredentials(userId);

            var btceClientApi = new BtceApiClientV3(apiCredentials.Key, apiCredentials.Secret);

            var btcePairInfo = new BtceApiPublicClientV3().GetTicker(pair);

            var orderData = new BtceOrder{
                Amount = model.Amount,
                Pair = pair,
                Price = model.Price,
                State = OrderState.Opened,
                Type = model.OrderType,
                StopProfitPrice = model.StopProfitPrice,
                UserId = userId,
                TradeType = type
            };

            if (orderData.Type == OrderTypes.Limit || orderData.Type == OrderTypes.Market){
                if (model.OrderType == OrderTypes.Market){
                    model.Price = type == TradeType.Buy ? btcePairInfo.Sell : btcePairInfo.Buy;
                }
                try{
                    var tradeResult = btceClientApi.Trade(pair, type, model.Price, model.Amount);

                    orderData.OrderId = tradeResult.OrderId;
                    orderData.DateCreated = DateTime.UtcNow;
                    if (tradeResult.OrderId == 0){
                        orderData.State = OrderState.Closed;
                        orderData.DateClosedOrCancelled = DateTime.UtcNow;
                    }

                    _context.BtceOrders.Add(orderData);
                    _context.SaveChanges();

                    return Json(new{success = true});
                }
                catch (BtceException exception){
                    return Json(new{success = false, error = exception.Message});
                }
            }

            if (model.OrderType == OrderTypes.StopLossMarket || model.OrderType == OrderTypes.TakeProfitMarket){
                model.Price = type == TradeType.Buy ? btcePairInfo.Sell : btcePairInfo.Buy;
            }

            orderData.State = OrderState.WaitForTrigger;

            _context.BtceOrders.Add(orderData);
            _context.SaveChanges();
            return Json(new{success = true});
        }

        public ActionResult CancelTradeBtce(int id){

            var apiCredentials = APIKeysManager.GetApiCredentials(User.Identity.GetUserId());

            var btceClientApi = new BtceApiClientV3(apiCredentials.Key, apiCredentials.Secret);

            try
            {
                var cancelResult = btceClientApi.CancelOrder(id);
                return Json(new{success = true, result = cancelResult});
            }
            catch (BtceException exception){
                return Json(new{success = false, error = exception.Message});
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp){
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}