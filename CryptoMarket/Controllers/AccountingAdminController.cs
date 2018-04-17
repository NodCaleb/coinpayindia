#region

using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class AccountingAdminController : Controller{
        // GET: AccountingAdmin
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(){
            return View(await AccountingManager.GetAvailableFeesAsync());
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<ActionResult> Withdraw(string id, double amount){
            await AccountingManager.WithdrawFeeToAdminAddressAsync(id, amount);
            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("{0} Fee withdraw, amount:{1}", CoinsManager.GetCoinNameById(id), amount), User.Identity.GetUserId(), Request.UserHostAddress);

            return RedirectToAction("Index");
        }
    }
}