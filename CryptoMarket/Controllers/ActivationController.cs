using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BtcE;
using CryptoMarket.Models;
using CryptoMarket.Source.Core.Platforms.BTC_e;
using Microsoft.AspNet.Identity;

namespace CryptoMarket.Controllers{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [RequireHttps]
    public class ActivationController : Controller{
        // GET: Activation
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult btce(){
            // 
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult btce(string key, string secret){
            try{
                var btceApi = new BtceApi(key, secret);
                var info = btceApi.GetInfo();
                if (info.Rights.Info && info.Rights.Trade){
                    APIKeysManager.SetApiCredentials(User.Identity.GetUserId(), key, secret);

                    return RedirectToAction("Index", "Platforms");
                }

                ViewBag.Error = true;
                return View();
            }
            catch{
                ViewBag.Error = true;
                return View();
            }
        }
    }
}