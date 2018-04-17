using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BtcE;
using CryptoMarket.Models.DB;

namespace CryptoMarket.Source.Core.Platforms.Database{
    /// <summary>
    /// 
    /// </summary>
    public class BtceOrder{

        /// <summary>
        /// 
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? DateClosedOrCancelled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BtcePair Pair { get; set; }

        public TradeType TradeType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OrderTypes Type { get; set; }

        public OrderState State { get; set; }

        public string UserId { get; set; }

        public decimal Amount { get; set; }

        public decimal Price { get; set; }

        public decimal StopProfitPrice { get; set; }

        public int OrderId { get; set; }
    }
}