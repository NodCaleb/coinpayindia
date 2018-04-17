#region

using System.Collections.Generic;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Models{
    /// <summary>
    /// 
    /// </summary>
    public class HomeViewModels{
        /// <summary>
        /// 
        /// </summary>
        public class IndexPageViewModel{
            /// <summary>
            /// 
            /// </summary>
            public List<Markets> BtcMarkets { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<Markets> LtcMarkets { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<Markets> IecMarkets { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<Markets> DogeMarkets { get; set; }



            /// <summary>
            /// 
            /// </summary>
            public double BtcValue { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double LtcValue { get; set; }


            /// <summary>
            /// 
            /// </summary>
            public double IecValue { get; set; }



            /// <summary>
            /// 
            /// </summary>
            public double DogeValue { get; set; }


            /// <summary>
            /// 
            /// </summary>
            public double TradesCount { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class FeesPageModel
        {
            /// <summary>
            /// </summary>
            public Dictionary<string, string> TradingFees { get; set; }

            /// <summary>
            /// </summary>
            public Dictionary<string, string> WithdrawFees { get; set; }
        }
    }
}