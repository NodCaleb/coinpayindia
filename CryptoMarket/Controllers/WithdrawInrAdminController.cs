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
    public class WithdrawInrAdminController : Controller
    {
        public ActionResult Index()
        {
            return View(WithdrawManager.GetPendingInrWithdrawals());
        }

        public ActionResult Proceed(string id)
        {
            return View(WithdrawManager.GetInrWithdrawById(id));
        }

        [System.Web.Http.HttpPost]
        public void Accept(string id)
        {
            WithdrawManager.InrWithdrawComplete(id);
        }

    }
}