using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CryptoMarket.Source.Core{
    /// <summary>
    /// 
    /// </summary>
    public enum OrderTypes{
        /// <summary>
        /// 
        /// </summary>
        Limit,

        /// <summary>
        /// 
        /// </summary>
        Market,

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Stop Loss Market")] StopLossMarket,

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Take Profit Market")] TakeProfitMarket,

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Stop Loss Limit")] StopLossLimit,

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Take Profit Limit")] TakeProfitLimit
    }
}