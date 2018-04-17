using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CryptoMarket.Models.DB {
    public class INRWithdrawRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAccountNumber { get; set; }

        public string BankName { get; set; }

        public string IFSCCode { get; set; }

        public double Amount { get; set; }

        public bool Executed { get; set; }

        public DateTime CreationDateTime { get; set; }

        public DateTime? ExecutionDateTime { get; set; }
    }
}