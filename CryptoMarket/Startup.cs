#region
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security.Cookies;
using Owin;

#endregion

namespace CryptoMarket {
    public partial class Startup {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app) {
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            ConfigureAuth(app);
            app.MapSignalR("/marketrealtime", new HubConfiguration {EnableDetailedErrors = true});      
        }
    }
}