#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    ///     Account Managing Controller
    /// </summary>
    [Authorize]
    [RequireHttps]
    public class AccountController : Controller{
        /// <summary>
        /// </summary>
        private ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        //
        // GET: /Account/Login
        /// <summary>
        /// </summary>
        private ApplicationSignInManager SignInManager => HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

        /// <summary>
        ///     GET: /Account/Login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl){
            if (User.Identity.IsAuthenticated){
                return RedirectToAction("Index", "Market");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        ///     POST: /Account/Login
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl){
            if (User.Identity.IsAuthenticated){
                return RedirectToAction("Index", "Market");
            }
            if (!ModelState.IsValid){
                return View(model);
            }
            var userdatas = await UserManager.FindByEmailAsync(model.Email);
            if (userdatas != null){
                if (userdatas.LastIPAccess != Request.UserHostAddress){
                    await UserManager.SendEmailAsync(userdatas.Id, "NEW IP Detected!", string.Format("Our system detected new IP accessed to account, {0}. Date(UTC): {1}", Request.UserHostAddress, DateTime.UtcNow));
                    await UserManager.ApplyNewIpAccessed(userdatas.Id, Request.UserHostAddress);
                }

                await UserManager.ApplyAccessDate(userdatas.Id);

                var result = await SignInManager.PasswordSignInAsync(userdatas.UserName, model.Password, model.RememberMe, false);
                switch (result){
                    case SignInStatus.Success:
                        await LogManager.WriteAsync(Logs.LogType.LoggedIn, "Successfully logged in.", userdatas.Id, Request.UserHostAddress);
                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new{ReturnUrl = returnUrl});
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        var userdata = await UserManager.FindByEmailAsync(model.Email);
                        if (userdata != null){
                            await LogManager.WriteAsync(Logs.LogType.FaliedLoggingIn, "Invalid logon request", userdata.Id, Request.UserHostAddress);
                        }
                        return View(model);
                }
            }
            ModelState.AddModelError("", "No accounts with that username!.");
            return View(model);
        }

        /// <summary>
        ///     GET: /Account/VerifyCode
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl){
            if (!await SignInManager.HasBeenVerifiedAsync()){
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(await SignInManager.GetVerifiedUserIdAsync());
            return View(new VerifyCodeViewModel{Provider = provider, ReturnUrl = returnUrl});
        }

        /// <summary>
        ///     POST: /Account/VerifyCode
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model){
            if (!ModelState.IsValid){
                return View(model);
            }
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, false, model.RememberBrowser == "on");
            switch (result){
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        /// <summary>
        ///     GET: /Account/Register
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Register(){
            if (User.Identity.IsAuthenticated){
                return RedirectToAction("Index", "Market");
            }

            return View();
        }

        /// <summary>
        ///     POST: /Account/Register
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model){
            if (User.Identity.IsAuthenticated){
                return RedirectToAction("Index", "Market");
            }

            if (ModelState.IsValid){
                var user = new ApplicationUser{
                    UserName = model.Username,
                    Email = model.Email,
                    PinCode = model.PinCode,
                    EmailConfirmed = true
                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded){
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new{userId = user.Id, code}, Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                    ViewBag.Link = callbackUrl;
                    return RedirectToAction("Index", "Market");
                }
                AddErrors(result);
            }
            return View(model);
        }



        /// <summary>
        ///     GET: /Account/ConfirmEmail
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code){
            if (userId == null || code == null){
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /// <summary>
        ///     GET: /Account/ForgotPassword
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ForgotPassword(){
            return View();
        }

        /// <summary>
        ///     POST: /Account/ForgotPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model){
            if (ModelState.IsValid){
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id))){
                    return View("ForgotPasswordConfirmation");
                }

                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new{userId = user.Id, code}, Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                return View("ForgotPasswordConfirmation");
            }
            return View(model);
        }


        /// <summary>
        ///     GET: /Account/ForgotPasswordConfirmation
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation(){
            return View();
        }

        /// <summary>
        ///     GET: /Account/ResetPassword
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code) {
            var model = new ResetPasswordViewModel{
                UserId = userId,
                TokenId = code
            };
            return code == null ? View("Error") : View(model);
        }

        /// <summary>
        ///     POST: /Account/ResetPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model){
            if (!ModelState.IsValid){
                return View(model);
            }

            var resetResult = await UserManager.ResetPasswordAsync(model.UserId, model.TokenId, model.Password);

            if (resetResult.Succeeded){
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(resetResult);
            return View();
        }

        /// <summary>
        ///     GET: /Account/ResetPasswordConfirmation
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation(){
            return View();
        }


        /// <summary>
        ///     GET: /Account/SendCode
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = "GoogleAuthenticator", ReturlUrl = returnUrl });
        }

        /// <summary>
        ///     POST: /Account/SendCode
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model) {
            if (!ModelState.IsValid) {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider)) {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new {
                Provider = model.SelectedProvider,
                model.ReturnUrl
            });
        }

        /// <summary>
        ///     POST: /Account/LogOff
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff(){
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Market");
        }


        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result){
            foreach (var error in result.Errors){
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl){
            if (Url.IsLocalUrl(returnUrl)){
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Market");
        }

        internal class ChallengeResult : HttpUnauthorizedResult{
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null){
            }

            public ChallengeResult(string provider, string redirectUri, string userId){
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context){
                var properties = new AuthenticationProperties{RedirectUri = RedirectUri};
                if (UserId != null){
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}