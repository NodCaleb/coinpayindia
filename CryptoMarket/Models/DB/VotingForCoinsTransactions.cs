#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    /// <summary>
    /// </summary>
    public class VotingForCoinsTransactions{
        /// <summary>
        /// 
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VoteId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TxId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime DateTime { get; set; }
    }
}