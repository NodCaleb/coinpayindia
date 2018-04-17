using System.Web.Mvc;

namespace CryptoMarket.Controllers {
    [RequireHttps]
    /// <summary>
    /// 
    /// </summary>
    public class DocsController : Controller {
        // GET: Docs
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index() {
            return View();
        }
    }
}