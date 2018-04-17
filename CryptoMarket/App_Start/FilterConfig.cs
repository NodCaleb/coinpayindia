#region

using System.Web.Mvc;
using CryptoMarket.Source;

#endregion

namespace CryptoMarket {
    /// <summary>
    /// </summary>
    public static class FilterConfig {
        /// <summary>
        /// </summary>
        /// <param name="filters"></param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new Filters.SessionTerminateBannedAccountFilter());
        }
    }
}