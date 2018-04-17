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
    public class WithdrawAdminController : Controller{
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(){
            ViewBag.DoneWithdraws = await WithdrawManager.GetAllDone();
            return View(await WithdrawManager.GetAllNonAuto());
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Accept(string id){
            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Withdraw Id {0} Accept", id), User.Identity.GetUserId(), Request.UserHostAddress);

            await WithdrawManager.Accept(id);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Decline(string id){
            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Withdraw Id {0} Decline", id), User.Identity.GetUserId(), Request.UserHostAddress);

            await WithdrawManager.Decline(id);
            return RedirectToAction("Index");
        }
    }
}