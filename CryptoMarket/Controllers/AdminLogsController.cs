#region

using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class AdminLogsController : Controller{
        #region Db-Init-Dispose
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        protected override void Dispose(bool disposing){
            if (disposing){
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        /// <summary>
        ///     GET: AdminLogs
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(){
            return View(await _db.Logs.Where(log => log.Type == Logs.LogType.AdminTrackAction).Take(256).ToListAsync());
        }
    }
}