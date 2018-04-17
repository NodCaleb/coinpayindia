#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    public class CoinSystems{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string EndpointIP { get; set; }
        public int EndpointPort { get; set; }
        public string EndpointLogin { get; set; }
        public string EndpointPassword { get; set; }
        public bool Active { get; set; }
        public int ConfirmationCointForDeposit { get; set; }
        public double WithdrawalFee { get; set; }

        public string AdminWallet { get; set; }

        public bool DepositDisabled { get; set; }

        public bool WithdrawDisabled { get; set; }

        public double MinimumTradeAmount { get; set; }
    }
}