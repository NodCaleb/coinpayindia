#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CryptoMarket.Source.Managers;

#endregion

namespace CryptoMarket.Models.DB{
    /// <summary>
    /// 
    /// </summary>
    public class AccountingLogs{
        /// <summary>
        /// 
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AccountingManager.AccountingLogType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }
    }
}