#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    public class DepositAddressesAssociations{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string CoinId { get; set; }
        public string Address { get; set; }
        public string Account { get; set; }
        public DateTime DateCreated { get; set; }
    }
}