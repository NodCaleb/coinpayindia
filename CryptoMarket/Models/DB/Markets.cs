#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    public class Markets{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string CoinFrom { get; set; }

        public string CoinTo { get; set; }

        public string PairName { get; set; }

        [Required]
        public double Fee { get; set; }

        public double? LatestPrice { get; set; }

        public double? PriceChangePercent { get; set; }

        public bool Active { get; set; }
    }
}