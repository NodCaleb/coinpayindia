using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoMarket.Models.DB {
    /// <summary>
    /// 
    /// </summary>
    public class PriceGraph {
        /// <summary>
        /// 
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MarketId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Period { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Price { get; set; }
    }
}