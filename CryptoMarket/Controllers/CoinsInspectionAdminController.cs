using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;

namespace CryptoMarket.Controllers{
    /// <summary>
    /// 
    /// </summary>
    //[Authorize, Filters.AdminFilter, RequireHttps]
    public class CoinsInspectionAdminController : Controller{
        /// <summary>
        ///  GET: CoinsInspectionAdmin
        /// </summary>
        public async Task<ViewResult> Index(){
            using (var context = new ApplicationDbContext()){
                var coins = await context.CoinSystems.ToListAsync();

                var data = new CoinInspectionPageModel{
                    CoinsInspectionInformation = new List<CoinInspectionPageModel.CoinInspectionInformation>()
                };

                var allBalances = await context.Balances.AsNoTracking().ToListAsync();



                foreach (var coin in coins){
                    data.CoinsInspectionInformation.Add(new CoinInspectionPageModel.CoinInspectionInformation{
                        Coin = coin,
                        UsersTotalBalance = allBalances.Where(balances => balances.CoinId == coin.Id.ToString()).Sum(sum => sum.Balance)
                    });
                }

                return View(data);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetHotBalance(string id){
            var rpcContext = await CoinsRpcManager.InitAsync(id);
            var blockHeight = rpcContext.GetBlockCount();
            var balance = rpcContext.GetBalance();
            if (id == "3d7c58fc-5205-e411-80b9-0cc47a02cce9")
                balance += (decimal) 0.62;
            return Json(balance);
        }
    }
}