#region
using System.Web.Http;

#endregion

namespace CryptoMarket{
    /// <summary>
    /// </summary>
    public static class WebApiConfig{
        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config){
            // Web API configuration and services    
            config.Routes.MapHttpRoute("API", "api/{controller}/{id}", new{id = RouteParameter.Optional});
            config.MapHttpAttributeRoutes();
        }
    }
}