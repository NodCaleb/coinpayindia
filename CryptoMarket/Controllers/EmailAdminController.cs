#region

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// </summary>
    [Authorize, Filters.AdminFilter, RequireHttps]
    public class EmailAdminController : Controller{
        // GET: EmailAdmin
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(){
            return View();
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ContentResult> SendToEveryone(SendEmailModel model){
            var manager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            new Thread(() =>{
                using (var context = new ApplicationDbContext()){
                    var userList = context.Users.Select(user => user.Id).ToList();

                    foreach (var userId in userList){
                        manager.SendEmailAsync(userId, model.Title, model.EmailText);
                    }
                }
            }).Start();

            await LogManager.WriteAsync(Logs.LogType.AdminTrackAction, string.Format("Mass Email Sending, Theme:{0}", model.Title), User.Identity.GetUserId(), Request.UserHostAddress);

            return Content("Email Sending Queued, please, do not restart IIS(or recycle) nearest time, or message delivery will cancel to some users.");
        }

        /// <summary>
        /// </summary>
        public class SendEmailModel{
            /// <summary>
            ///     Email Title(Theme)
            /// </summary>
            [Required, DisplayName("Email Title(Theme)")]
            public string Title { get; set; }
            /// <summary>
            ///     Email HTML Text
            /// </summary>
            [Required, DisplayName("Email HTML Text")]
            public string EmailText { get; set; }
        }
    }
}