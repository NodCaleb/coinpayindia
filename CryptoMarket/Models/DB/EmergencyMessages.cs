using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoMarket.Models.DB{
    public class EmergencyMessages{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime DateCreated { get; set; }

        public string Text { get; set; }

        public bool Active { get; set; }
    }
}