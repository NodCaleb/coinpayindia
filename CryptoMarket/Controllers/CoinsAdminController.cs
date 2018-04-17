#region

using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;
using CryptoMarket.Source.Core.CustomCoinsProtocols;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class CoinsAdminController : Controller{
        // GET: CoinsAdmin
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(){
            using (var context = new ApplicationDbContext()){
                return View(await context.CoinSystems.ToListAsync());
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create(){
            return View();
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CoinSystems model){
            if (!Regex.IsMatch(model.EndpointIP, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$")){
                ModelState.AddModelError("", "IP is Invalid");
                return View(model);
            }

            if (model.EndpointPort < 0 && model.EndpointPort > 65535){
                ModelState.AddModelError("", "PORT is Invalid. Range: 0 - 65535");
                return View(model);
            }

            if (model.ShortName == "NXT" || model.Name == "NXTCoin"){
                if (!NxtCoinProtocol.TryInit()){
                    ModelState.AddModelError("", "Could not connect to NXTCoin API, check if it is installed and running on '127.0.0.1:7876'");
                    return View(model);
                }
            }
            else if (model.ShortName == "USD" || model.Name == "Dollar"){
            }
            else if (!await CoinsRpcManager.CheckAvailability(model.EndpointIP, model.EndpointPort, model.EndpointLogin, model.EndpointPassword)){
                ModelState.AddModelError("", "Could not connect to coin RPC Api. Check connection details and credentials");
                return View(model);
            }

            using (var context = new ApplicationDbContext()){
                model.Active = true;
                context.CoinSystems.Add(model);
                await context.SaveChangesAsync();
            }

            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("New Coin Created:{0}", model.ShortName), User.Identity.GetUserId(), Request.UserHostAddress);

            return RedirectToAction("Index", "CoinsAdmin");
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        [HttpGet]
        public async Task<ActionResult> Edit(string id){
            return View(await CoinsManager.GetAsync(id));
        }

        /// <summary>
        /// Enable/Disable withdraw ability
        /// </summary>
        /// <param name="id"></param>
        [HttpGet]
        public async Task<ActionResult> ToggleWithdraw(string id){
            await CoinsManager.ToggleWithdraw(id);  
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Enable/Disable deposit ability
        /// </summary>
        /// <param name="id"></param>
        [HttpGet]
        public async Task<ActionResult> ToggleDeposit(string id)
        {
            await CoinsManager.ToggleDeposit(id);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CoinSystems model){
            if (!Regex.IsMatch(model.EndpointIP, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$")){
                ModelState.AddModelError("", "IP is Invalid");
                return View(model);
            }

            if (model.EndpointPort < 0 && model.EndpointPort > 65535){
                ModelState.AddModelError("", "PORT is Invalid. Range: 0 - 65535");
                return View(model);
            }

            if (!await CoinsRpcManager.CheckAvailability(model.EndpointIP, model.EndpointPort, model.EndpointLogin, model.EndpointPassword)){
                ModelState.AddModelError("", "Could not connect to coin RPC Api. Check connection details and credentials");
                return View(model);
            }

            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Coin Edited:{0}", model.ShortName), User.Identity.GetUserId(), Request.UserHostAddress);

            await CoinsManager.EditAsync(model);

            return RedirectToAction("Index");
        }
    }
}