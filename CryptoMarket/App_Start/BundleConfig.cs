#region

using System.Web.Optimization;

#endregion

namespace CryptoMarket {
    /// <summary>
    /// 
    /// </summary>
    public static class BundleConfig {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundles"></param>
        public static void RegisterBundles(BundleCollection bundles) {


            bundles.Add(new StyleBundle("~/assets/styles").Include(
                "~/assets/css/core.css",
                "~/assets/css/components.css",
                "~/assets/css/icons.css",
                "~/assets/css/pages.css",
                "~/assets/css/menu.css",
                "~/assets/css/responsive.css"));




            bundles.Add(new StyleBundle("~/jscripts").Include(
                "~/assets/js/modernizr.min.js",
                "~/assets/js/detect.js",
                "~/assets/js/fastclick.js",
                "~/assets/js/jquery.slimscroll.js",
                "~/assets/js/jquery.blockUI.js",
                "~/assets/js/waves.js",
                "~/assets/js/wow.min.js",
                "~/assets/js/jquery.nicescroll.js",
                "~/assets/js/jquery.scrollTo.min.js"));


        }
    }
}