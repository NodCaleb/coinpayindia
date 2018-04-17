#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Base32;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using OtpSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Web;
using System.IO;
using System.Text;

#endregion

namespace CryptoMarket {
    /// <summary>
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser> {
        /// <summary>
        /// </summary>
        /// <param name="store"></param>
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store) {
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pincode"></param>
        /// <returns></returns>
        public async Task<bool> ValidatePinCode(string userId, string pincode) {
            using (var context = new ApplicationDbContext()) {
                return await context.Users.AnyAsync(user => user.Id.ToString() == userId && user.PinCode == pincode);
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task ApplyNewIpAccessed(string userId, string ip) {
            using (var context = new ApplicationDbContext()) {
                var userData = await context.Users.FirstAsync(user => user.Id.ToString() == userId);
                userData.LastIPAccess = ip;
                context.Entry(userData).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task ApplyAccessDate(string userId) {
            using (var context = new ApplicationDbContext()) {
                var userData = await context.Users.FirstAsync(user => user.Id.ToString() == userId);
                userData.LastDateAccess = DateTime.UtcNow;
                context.Entry(userData).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context) {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            manager.UserValidator = new UserValidator<ApplicationUser>(manager) {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            manager.PasswordValidator = new PasswordValidator {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };

            manager.UserLockoutEnabledByDefault = false;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;



            manager.RegisterTwoFactorProvider("GoogleAuthenticator", new GoogleAuthenticatorTokenProvider());

            manager.EmailService = new EmailService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null) {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("Exchange Protection - Weig Rate"));
            }
            return manager;
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<List<Order>> GetUserOrders(string userId) {
            using (var context = new ApplicationDbContext()) {
                return await context.Orders.Where(user => user.UserId == userId).ToListAsync();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsGoogleTwoFactorEnabled(string userName) {
            using (var context = new ApplicationDbContext()) {
                return context.Users.First(user => user.UserName == userName).IsGoogleAuthenticatorEnabled;
            }
        }

        public static string SensSMSIndiaMessage(string phoneNo, string message)
        {
            string url = "http://login.bulksmsgateway.in/sendmessage.php";
            string result = "";
            message = HttpUtility.UrlPathEncode(message);
            String strPost = "?user=" + HttpUtility.UrlPathEncode("YOCOIL") + "&password=" + HttpUtility.UrlPathEncode("7302456") + "&sender=" + HttpUtility.UrlPathEncode("COINPA") + "&mobile=" + HttpUtility.UrlPathEncode(phoneNo) + "&type=" + HttpUtility.UrlPathEncode("3") + "&message=" + message;
            StreamWriter myWriter = null;
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url + strPost);
            objRequest.Method = "POST";
            objRequest.ContentLength = Encoding.UTF8.GetByteCount(strPost);
            objRequest.ContentType = "application/x-www-form-urlencoded";
            try
            {
                myWriter = new StreamWriter(objRequest.GetRequestStream());
                myWriter.Write(strPost);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                myWriter.Close();
            }
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
            {
                result = sr.ReadToEnd();
                // Close and clean up the StreamReader sr.Close();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsUserAdmin(string userId) {
            return userId == "a0620a5a-1f57-49ec-95a0-64ab04c6a99a" || userId == "ed6aaa00-bd8e-4a26-b533-06555e1c33de";
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetUserEmail(string userId) {
            using (var context = new ApplicationDbContext()) {
                return context.Users.First(user => user.Id == userId).Email;
            }
        }
    }

    /// <summary>
    /// </summary>
    public class ApplicationRoleManager : RoleManager<IdentityRole> {
        /// <summary>
        /// </summary>
        /// <param name="roleStore"></param>
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore) {
        }

        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context) {
            return new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>()));
        }
    }

    /// <summary>
    /// </summary>
    public class GoogleAuthenticatorTokenProvider : IUserTokenProvider<ApplicationUser, string> {
        public Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser, string> manager, ApplicationUser user) {
            return Task.FromResult((string)null);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser, string> manager, ApplicationUser user) {
            long timeStepMatched;

            var otp = new Totp(Base32Encoder.Decode(user.GoogleAuthenticatorSecretKey));
            var valid = otp.VerifyTotp(token, out timeStepMatched, new VerificationWindow(2, 2));

            return Task.FromResult(valid);
        }

        public Task NotifyAsync(string token, UserManager<ApplicationUser, string> manager, ApplicationUser user) {
            return Task.FromResult(true);
        }

        public Task<bool> IsValidProviderForUserAsync(UserManager<ApplicationUser, string> manager, ApplicationUser user) {
            return Task.FromResult(user.IsGoogleAuthenticatorEnabled);
        }
    }


    /// <summary>
    /// </summary>
    public class EmailService : IIdentityMessageService {
        /// <inheritdoc />
        public Task SendAsync(IdentityMessage message) {
            try {
                var client = new SendGridClient("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");
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("support@coinpayindia.com", "CoinPayIndia Exchange"),
                    Subject = message.Subject,
                    HtmlContent = message.Body
                };
                msg.AddTo(new EmailAddress(message.Destination, message.Destination));
                var response = client.SendEmailAsync(msg).ConfigureAwait(false);

            } catch (Exception exception) {
                //throw new Exception(exception.Message);
            }
            return Task.FromResult(0);
        }
    }


    /// <summary>
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string> {
        /// <summary>
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="authenticationManager"></param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager) {
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user) {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context) {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}