using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity.Owin;

namespace CryptoMarket.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class VerificationAdminController : Controller
    {
        private ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        private ApplicationDbContext Context => new ApplicationDbContext();

        private VerificationManager VerificationManager => new VerificationManager(Context,UserManager);

        // GET: VerificationAdmin
        public ActionResult Index()
        {
            return View(VerificationManager.GetUsersForVerification());
        }

        public ActionResult ShowImage(string id) {
            object mod = id;

            return File(Server.MapPath("~/images/documents/" + id), "image/png");

        }

        public ActionResult Accept(string userId, string fullName) {
            VerificationManager.SetDocumentVerification(userId, fullName);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Reject(string id) {
            var verification = VerificationManager.SetVerificationLevel(id, VerificationManager.VerificationLevel.Phone);

            return RedirectToAction("Index");
        }
    }
}