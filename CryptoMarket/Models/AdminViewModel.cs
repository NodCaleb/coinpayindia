#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Models{
    /// <summary>
    /// 
    /// </summary>
    public class RoleViewModel{
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Role Name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EditUserViewModel{
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SelectListItem> RolesList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Banned { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CoinInspectionPageModel
    {
        /// <summary>
        /// 
        /// </summary>
        public class CoinInspectionInformation
        {
            /// <summary>
            /// 
            /// </summary>
            public CoinSystems Coin { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double UsersTotalBalance { get; set; }

        }

        /// <summary>
        /// 
        /// </summary>
        public List<CoinInspectionInformation> CoinsInspectionInformation { get; set; }
    }
}