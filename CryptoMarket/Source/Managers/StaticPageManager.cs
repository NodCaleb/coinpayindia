#region

using System.Collections.Generic;
using System.Linq;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Source.Managers{
    public static class StaticPageManager{
        public static IEnumerable<StaticPages> GetAll(){
            using (var context = new ApplicationDbContext()){
                return context.StaticPages.ToList();
            }
        }

        public static StaticPages Get(string url){
            using (var context = new ApplicationDbContext()){
                return context.StaticPages.First(pages => pages.Url == url);
            }
        }
    }
}