#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CryptoMarket.Source.Core;

#endregion

namespace CryptoMarket.Models.DB{
    /// <summary>
    /// 
    /// </summary>
    public enum TradeType{
        /// <summary>
        /// 
        /// </summary>
        Buy,
        /// <summary>
        /// 
        /// </summary>
        Sell
    }

    /// <summary>
    /// 
    /// </summary>
    public class Order{
        /// <summary>
        /// 
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TradeType TradeType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OrderTypes OrderType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MarketId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Amount { get; set; } // TOTAL Amount from beginning
        /// <summary>
        /// 
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double ActionPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Total { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double PartialOrderTotalLeft { get; set; } // This shit calculates amount which is AFTER partial ordering.
        /// <summary>
        /// 
        /// </summary>
        public double FeeTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double NetTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DateClosed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Closed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CloserUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClosingOrderId { get; set; }
    }
}