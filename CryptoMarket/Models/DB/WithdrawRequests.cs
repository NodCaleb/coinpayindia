#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    public class WithdrawRequests{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string CoinId { get; set; }

        public string UserId { get; set; }

        public double Amount { get; set; }

        public DateTime DateCreated { get; set; }

        public bool Paid { get; set; }

        public DateTime? DatePaid { get; set; }

        public string Address { get; set; }

        public bool Auto { get; set; }

        public string Ip { get; set; }

        public string TxId { get; set; }
    }
}