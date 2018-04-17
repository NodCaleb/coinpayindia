#region

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    /// <summary>
    /// </summary>
    public class Vouchers{
        /// <summary>
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Voucher Code")]
        public string VoucherCode { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Created By")]
        public string CreatorUserId { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Redeemed By")]
        public string RedeemerUserId { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Involved Coin")]
        public string CoinId { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Amount")]
        public double Amount { get; set; }

        /// <summary>
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Already Used?")]
        public bool Redeemed { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Creation Date")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Expiration Date")]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Redeem Code")]
        public DateTime? RedeemDate { get; set; }
    }
}