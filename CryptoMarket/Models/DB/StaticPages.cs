#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    public class StaticPages{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public DateTime LastModified { get; set; }

        public string UserIdLastChanger { get; set; }
    }
}