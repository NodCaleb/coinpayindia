#region

using System.Web;
using System.Web.Mvc;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;

#endregion

namespace CryptoMarket.Source{
    /// <summary>
    /// </summary>
    public class Filters{
        /// <summary>
        /// </summary>
        public class SessionTerminateBannedAccountFilter : IResultFilter{
            public void OnResultExecuting(ResultExecutingContext filterContext){
            }

            public void OnResultExecuted(ResultExecutedContext filterContext){
                if (HttpContext.Current.User.Identity.IsAuthenticated){
                    if (UsersManager.IsUserLocked(HttpContext.Current.User.Identity.GetUserId())){
                        filterContext.HttpContext.Response.RedirectLocation = "/account/logoff";
                        filterContext.HttpContext.Response.StatusCode = 302;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class AdminFilter : ActionFilterAttribute, IActionFilter{
           void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext){
               if (!HttpContext.Current.User.Identity.IsAuthenticated) {
                   filterContext.HttpContext.Response.StatusCode = 404;
               }


               if (!ApplicationUserManager.IsUserAdmin(HttpContext.Current.User.Identity.GetUserId())){  
                   filterContext.HttpContext.Response.StatusCode = 404;
               }    
            }
        }
    }
}