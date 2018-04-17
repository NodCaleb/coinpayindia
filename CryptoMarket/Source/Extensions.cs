#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

#endregion

namespace CryptoMarket.Source{
    /// <summary>
    /// </summary>
    public static class Extensions{

        /// <summary>
        /// Getting Current Software Version
        /// </summary>
        public static string SoftwareVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToMD5(this string value){
            var data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
            var sBuilder = new StringBuilder();
            foreach (var t in data){
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Sha256(this string value){
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = new SHA256Managed().ComputeHash(bytes);
            return hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="property"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property) {
            return items.GroupBy(property).Select(x => x.First());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source) {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            foreach (var element in source) {

                target.Add(element);
            }

        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Base64(this string value){
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        ///     Checks the current action via RouteData
        /// </summary>
        /// <param name="helper">The HtmlHelper object to extend</param>
        /// <param name="actionName">The Action</param>
        /// <param name="controllerName">The Controller</param>
        /// <returns>Boolean</returns>
        public static bool IsCurrentAction(this HtmlHelper helper, string actionName, string controllerName){
            var currentControllerName = (string) helper.ViewContext.RouteData.Values["controller"];
            var currentActionName = (string) helper.ViewContext.RouteData.Values["action"];

            return currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) && currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}