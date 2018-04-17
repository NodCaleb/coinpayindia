using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CryptoMarket.Models;
using Quartz;

namespace CryptoMarket.Source.Managers {
    /// <summary>
    /// 
    /// </summary>
    public class DynamicFeeManager {

        /// <summary>
        /// Parties of exchange process
        /// </summary>
        public enum Parties {
            /// <summary>
            /// Creates iqudity of exchange
            /// </summary>
            Maker,
            /// <summary>
            /// Remove liqidity from exchange
            /// </summary>
            Taker
        } 



        public static async Task UserTraded(string userId) {
            using (var context = new ApplicationDbContext()) {
                var userInfo = await context.Users.FirstAsync(_ => _.Id.ToString() == userId);
                var userOrders = await context.Orders.AsNoTracking().Where(_ => _.UserId == userId && _.Closed && _.DateClosed.Value > DateTime.UtcNow.AddMonths(-1)).ToListAsync();
            }  
        }
    }
}