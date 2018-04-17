using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using CryptoMarket.Models;

namespace CryptoMarket.Controllers{
    /// <summary>
    /// 
    /// </summary>
    public class BlogController : Controller{
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ViewResult> Index(){
            using (var context = new ApplicationDbContext()){
                return View(await context.BlogPosts.ToListAsync());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ViewResult> Post(string id){
            using (var context = new ApplicationDbContext()){
                return View(await context.BlogPosts.FirstAsync(post => post.Id.ToString() == id));
            }
        }
    }
}