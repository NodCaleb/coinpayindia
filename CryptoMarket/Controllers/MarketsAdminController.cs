#region

using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
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
    public class MarketsAdminController : Controller{
        /// <summary>
        ///     MarketsAdmin
        /// </summary>
        public async Task<ActionResult> Index(){
            using (var context = new ApplicationDbContext()){
                return View(await context.Markets.ToListAsync());
            }
        }

        /// <summary>
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create(){
            using (var context = new ApplicationDbContext()) {
                ViewBag.FromCoins = await context.CoinSystems.Where(_ => _.ShortName == "ETH" || _.ShortName == "BTC" || _.ShortName == "INR").ToListAsync();

                ViewBag.CoinSystemsList = await context.CoinSystems.ToListAsync();
                return View();
            }
        }



        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Markets model){
            using (var context = new ApplicationDbContext()){
                if (model.CoinFrom == model.CoinTo){
                    ModelState.AddModelError("", "Coins must not be the same");
                    ViewBag.CoinSystemsList = await context.CoinSystems.ToListAsync();
                    return View(model);
                }

                if (await context.Markets.AnyAsync(market => market.CoinFrom == model.CoinFrom && market.CoinTo == model.CoinTo)){
                    ModelState.AddModelError("", "This market already exists");
                    ViewBag.CoinSystemsList = await context.CoinSystems.ToListAsync();
                    return View(model);
                }

                model.PairName = $"{CoinsManager.GetCoinShortNameById(model.CoinTo)}/{CoinsManager.GetCoinShortNameById(model.CoinFrom)}";
                model.LatestPrice = 0;
                model.PriceChangePercent = 0;
                model.Active = true;
                context.Markets.Add(model);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ToggleActive(string id){
            using (var context = new ApplicationDbContext()){
                var marketData = await context.Markets.FirstAsync(markets => markets.Id.ToString() == id);
                marketData.Active = !marketData.Active;

                context.Entry(marketData).State = EntityState.Modified;

                await context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marketId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task DeleteMarket(string marketId){
            await OrdersManager.DeleteAllMarketOrders(marketId);
            await MarketsManager.DeleteMarket(marketId);
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Details(string id){
            using (var context = new ApplicationDbContext()){
                ViewBag.Orders = await context.Orders.Where(orde => orde.MarketId == id).ToListAsync();
                var dayAgo = DateTime.UtcNow.AddDays(-1);

                ViewBag.DayVolume = 0.0;
                if (await context.Orders.AnyAsync(orde => orde.MarketId == id && orde.DateClosed > dayAgo)){
                    ViewBag.DayVolume = await context.Orders.Where(orde => orde.MarketId == id && orde.DateClosed > dayAgo).SumAsync(summ => summ.Total);
                }

                ViewBag.TotalVolume = 0.0;
                if (await context.Orders.AnyAsync(orde => orde.MarketId == id)){
                    ViewBag.TotalVolume = await context.Orders.Where(orde => orde.MarketId == id).SumAsync(summ => summ.Total);
                }

                return View(await context.Markets.FirstAsync(market => market.Id.ToString() == id));
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Edit(string id){
            using (var context = new ApplicationDbContext()){
                return View(await context.Markets.FirstAsync(market => market.Id.ToString() == id));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSubmit(Markets model){
            using (var context = new ApplicationDbContext()){
                var oldMarket = await context.Markets.FirstAsync(market => market.Id == model.Id);

                oldMarket.Fee = model.Fee;

                context.Entry(oldMarket).State = EntityState.Modified;

                await context.SaveChangesAsync();

                return RedirectToAction("Details", new{id = model.Id});
            }
        }
    }
}