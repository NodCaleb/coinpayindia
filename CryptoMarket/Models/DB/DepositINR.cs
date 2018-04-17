#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    public class DepositINR{

        public enum DepositInrStatus
        {
            Waiting,
            Accepted,
            Rejected
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string UserFullName { get; set; }

        public double Amomunt { get; set; }

        public string SlipImageGuid { get; set; }

        public DepositInrStatus Status { get; set; } 
    }
}