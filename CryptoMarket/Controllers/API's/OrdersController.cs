#region

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.UI;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Core;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// Get Order Information
    /// </summary>
    public class OrdersController : ApiController{


        /// <summary>
        /// Getting all active orders on whole exchange.
        /// </summary>
        /// <returns></returns>
        //[OutputCache(Duration = 3, Location = OutputCacheLocation.Server)]
        //public async Task<IEnumerable<Order>> Get() {
        //    return await OrdersManager.GetOrders();
        //}

        /// <summary>
        /// Get all orders based on requested market id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Order> Get(string id) {
            return OrdersManager.GetOrders(id);
        }

        /// <summary>
        /// Order Creation
        /// </summary>

        /// <param name="data"></param>
        /// <returns></returns>
        public JsonResult<OrdersManager.OrderCreateResult> Post([FromBody]OrderCreateRequest data) {
            return Json(OrdersManager.Create(data.marketId, User.Identity.GetUserId(), data.amount, data.price, data.orderType, data.tradeType, data.actionPrice));
        }


        /// <summary>
        /// 
        /// </summary>
        public class OrderCreateRequest {
            /// <summary>
            /// 
            /// </summary>
            public  string marketId { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public double amount { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public double price { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public OrderTypes orderType { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public TradeType tradeType { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public double actionPrice { get; set; }
        }
    }
}