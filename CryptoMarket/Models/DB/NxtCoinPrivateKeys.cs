#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    /// <summary>
    /// </summary>
    public class NxtCoinPrivateKeys{
        /// <summary>
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// </summary>
        public string PrivateKey { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class EthCoinPrivateKeys {
        /// <summary>
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// </summary>
        public string PrivateKey { get; set; }
    }
}