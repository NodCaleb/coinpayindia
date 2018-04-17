#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    public class AccountingFees{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string CoinId { get; set; }

        public double AvailableAmount { get; set; }
    }
}