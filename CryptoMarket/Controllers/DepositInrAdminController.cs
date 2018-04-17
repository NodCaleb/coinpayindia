using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Http;

namespace CryptoMarket.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [System.Web.Http.Authorize, Filters.AdminFilter, RequireHttps]
    public class DepositInrAdminController : Controller
    {
        // GET: VerificationAdmin
        public ActionResult Index()
        {
            return View(DepositsManager.GetPendingInrDeposits());
        }

        public ActionResult Proceed(string id)
        {
            return View(DepositsManager.GetInrDeposit(id));
        }

        public ActionResult GetImage(string id)
        {
            return File(Server.MapPath("~/images/bankslip/" + id), "image/png");
        }

        [System.Web.Http.HttpPost]
        public void Accept([FromBody]string id, [FromBody]double amoumt)
        {
            DepositsManager.AcceptInrDeposit(id, amoumt);
        }

        [System.Web.Http.HttpPost]
        public void Reject(string id)
        {
            DepositsManager.RejectInrDeposit(id);
        }
    }
}