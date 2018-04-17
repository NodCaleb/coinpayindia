using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;

namespace CryptoMarket.Controllers{
    /// <summary>
    /// 
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class VouchersAdminController : Controller{

        /// <summary>
        /// GET: VouchersAdmin
        /// </summary>
        /// <returns></returns>
        public async Task<ViewResult> Index(){
            return View(await VouchersManager.GetAll());
        }
    }
}