#region

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace CryptoMarket.Models.DB{
    /// <summary>
    /// </summary>
    public class Logs{
        /// <summary>
        /// </summary>
        public enum LogType{
            /// <summary>
            /// </summary>
            LoggedIn,
            /// <summary>
            /// </summary>
            FaliedLoggingIn,
            /// <summary>
            ///     Action, describes any other admin action
            /// </summary>
            AdminTrackAction
        }

        /// <summary>
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("User Id")]
        public string UserId { get; set; }

        /// <summary>
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// </summary>
        public LogType Type { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Log Text")]
        public string Text { get; set; }

        /// <summary>
        /// </summary>
        [DisplayName("Date Time")]
        public DateTime DateTime { get; set; }
    }
}