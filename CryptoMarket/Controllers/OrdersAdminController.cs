#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class OrdersAdminController : Controller{
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        /// <summary>
        ///     GET: OrdersAdmin
        /// </summary>
        /// <returns></returns>
        public async Task<ViewResult> Index(){
            var allMarkets = await _db.Markets.ToListAsync();

            var ordersUnFormatted = await _db.Orders.ToListAsync();

            var ordersFormatted = new List<Order>();
            foreach (var order in ordersUnFormatted){
                order.MarketId = allMarkets.First(market => market.Id.ToString() == order.MarketId).PairName;
                ordersFormatted.Add(order);
            }

            return View(ordersFormatted);
        }


        /// <summary>
        ///     GET: OrdersAdmin/Details/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(Guid? id){
            if (id == null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var orders = _db.Orders.Find(id);
            if (orders == null){
                return HttpNotFound();
            }
            return View(orders);
        }

        /// <summary>
        ///     GET: OrdersAdmin/Remove
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Remove(string id, string userId){
            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Deleted Order With Id: {0}", id), User.Identity.GetUserId(), Request.UserHostAddress);
            OrdersManager.Delete(id, userId);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing){
            if (disposing){
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}