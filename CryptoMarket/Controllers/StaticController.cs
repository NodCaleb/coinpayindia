#region

using System.Web.Mvc;
using CryptoMarket.Source.Managers;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
    public class StaticController : Controller{
        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ActionResult Page(string url){
            ViewBag.Text = StaticPageManager.Get(url).Text;
            return View();
        }
    }
}