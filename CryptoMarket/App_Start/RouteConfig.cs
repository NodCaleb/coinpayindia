#region

using System.Web.Mvc;
using System.Web.Routing;

#endregion

namespace CryptoMarket {
    /// <summary>
    /// </summary>
    public static class RouteConfig {
        /// <summary>
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Trade", "trade/{*pair}", new { controller = "Market", action = "Index", pair = UrlParameter.Optional });
            routes.MapRoute("Voting", "voting", new { controller = "Market", action = "Voting", pair = UrlParameter.Optional });
            routes.MapRoute("Faq", "faq", new { controller = "Market", action = "Faq", pair = UrlParameter.Optional });
            routes.MapRoute("Fees", "fees", new { controller = "Market", action = "Fees", pair = UrlParameter.Optional });

            routes.MapRoute("Support", "support", new { controller = "Market", action = "Support", pair = UrlParameter.Optional });


            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Market", action = "Index", id = UrlParameter.Optional });

            routes.LowercaseUrls = true;
        }
    }
}