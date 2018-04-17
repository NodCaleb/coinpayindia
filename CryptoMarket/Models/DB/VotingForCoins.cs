#region

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    /// <summary>
    /// </summary>
    public class VotingForCoins{
        /// <summary>
        /// 
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("'Vote for' Coin Name")]
        public string CoinName { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Address for Vote")]
        public string VotingAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("One vote Price")]
        public double Price { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DisplayName("Coin, used to vote")]
        public string CoinUsed { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Current Votes Count")]
        public int CurrentVotesNumber { get; set; }

        /// <summary>
        ///     Is Vote active now?
        /// </summary>
        [DisplayName("Active?")]
        public bool Active { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VotingAccount { get; set; }
    }
}