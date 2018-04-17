#region

using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Core.Platforms.Database;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

#endregion

namespace CryptoMarket.Models {
    /// <summary>
    /// </summary>
    public class ApplicationUser : IdentityUser {
        /// <summary>
        ///     Google 2fa enable?
        /// </summary>
        public bool IsGoogleAuthenticatorEnabled { get; set; }

        /// <summary>
        ///     Google Secret Temp Code
        /// </summary>
        public string GoogleAuthenticatorSecretKey { get; set; }

        /// <summary>
        ///     User's Pin Code
        /// </summary>
        public string PinCode { get; set; }

        /// <summary>
        ///     Last access with PIN
        /// </summary>
        public string LastIPAccess { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastDateAccess { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool BtceActive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BtceKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BtceSecret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public VerificationManager.VerificationLevel VerificationLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PhoneNumberVerificationCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string VerificationDocumentImageGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Fullname { get; set; }


        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager) {
            return await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        }
    }

    /// <summary>
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
         
        /// <summary>
        /// </summary>
        public ApplicationDbContext() : base("DefaultConnection", false) {
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = false;
        }

        /// <summary>
        /// </summary>
        public DbSet<CoinSystems> CoinSystems { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<DepositAddressesAssociations> DepositAddressesAssociations { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<Balances> Balances { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<Markets> Markets { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<DepositsTransactions> DepositsTransactions { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<WithdrawRequests> WithdrawRequests { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<Logs> Logs { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<GraphStats> GraphStats { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<PriceGraph> PriceGraphs { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<StaticPages> StaticPages { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<AccountingLogs> AccountingLogs { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<AccountingFees> AccountingFees { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<WebsiteBooleanStates> WebsiteBooleanStates { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<VotingForCoins> VotingForCoins { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<VotingForCoinsTransactions> VotingForCoinsTransactions { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<Vouchers> Vouchers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<PersonalMessagesManager.Message> PersonalMessages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<VerificationManager.DayUsedWithdrawLimit> WithdrawLimits { get; set; }


        /// <summary>
        /// </summary>
        public DbSet<NxtCoinPrivateKeys> NxtCoinPrivateKeys { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<EthCoinPrivateKeys> EthCoinPrivateKeys { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<BlogPosts> BlogPosts { get; set; }

        /// <summary>
        /// </summary>
        public DbSet<DepositINR> DepositsINR { get; set; }

        public DbSet<INRWithdrawRequest> INRWithdrawRequests { get; set; }


        /// <summary>
        /// </summary>
        public DbSet<EmergencyMessages> EmergencyMessages { get; set; }


        /// <summary>
        /// </summary>
        public DbSet<BtceOrder> BtceOrders { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static ApplicationDbContext Create() {
            return new ApplicationDbContext();
        }
    }
}