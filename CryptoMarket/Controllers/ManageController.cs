#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Base32;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using CryptoMarket.Source.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OtpSharp;

#endregion

namespace CryptoMarket.Controllers{
    /// <summary>
    /// 
    /// </summary>
    [System.Web.Mvc.Authorize]
    [RequireHttps]
    public class ManageController : Controller{
        /// <summary>
        /// </summary>
        private ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        /// <summary>
        ///     GET: /Account/Index
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(){
            return View();
        }


        /// <summary>
        /// Viewpage of Personal Messaging System
        /// </summary>
        /// <returns></returns>
        public ActionResult Messages(string id=null) {
            return id == null ? View("Messages", null) : View(UserManager.FindById(id));
        }

        public ActionResult Verification() {
            using (var context = new ApplicationDbContext()) {
                var verificationLevel = new VerificationManager(context,UserManager).GetLevel(User.Identity.GetUserId());
                return View(verificationLevel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phoneNum"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult VerificationSMSSend(string phoneNum) {
            using (var context = new ApplicationDbContext()) {
                return Json(new VerificationManager(context, UserManager).SendVerificationCodeToPhone(User.Identity.GetUserId(), phoneNum));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phoneNum"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult VerificationCodeCheck(string code) {
            using (var context = new ApplicationDbContext()) {
                return Json(new VerificationManager(context, UserManager).CheckPhoneNumberVerification(User.Identity.GetUserId(), code));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult UploadDocument(HttpPostedFileBase file) {
            if (file != null) {
                string pic = System.IO.Path.GetFileName(file.FileName);
                var guidrandom = Guid.NewGuid().ToString();
                string path = System.IO.Path.Combine(
                    Server.MapPath("~/images/documents"), guidrandom);
                // file is uploaded
                file.SaveAs(path);

                using (var context = new ApplicationDbContext()) {
                    new VerificationManager(context, UserManager).InitiateIdVerification(guidrandom, User.Identity.GetUserId());
                }

            }
            // after successfully uploading redirect the user
            return RedirectToAction("Verification", "Manage");
        }


        public ActionResult UploadInrSlip(HttpPostedFileBase file, [FromBody] double Amomunt)
        {
            using (var context = new ApplicationDbContext())
            {
                var inrCoin = context.CoinSystems.First(_ => _.ShortName == "INR");

                if (file != null)
                {
                    string pic = System.IO.Path.GetFileName(file.FileName);
                    var guidrandom = Guid.NewGuid().ToString();
                    string path = System.IO.Path.Combine(
                        Server.MapPath("~/images/bankslip"), guidrandom);
                    // file is uploaded
                    file.SaveAs(path);

                    var userId = User.Identity.GetUserId();

                    var userInfo = context.Users.First(_ => _.Id == userId);

                    context.DepositsINR.Add(new DepositINR
                    {
                        Amomunt = Amomunt,
                        SlipImageGuid = guidrandom,
                        Status = DepositINR.DepositInrStatus.Waiting,
                        UserFullName = userInfo.Fullname,
                        UserId = userId
                    });

                    context.SaveChanges();

                }
                // after successfully uploading redirect the user
                return RedirectToAction("Deposit", "Manage", new { coinId = (object)inrCoin.Id });
            }
        }


        [System.Web.Http.HttpPost]
        public JsonResult UploadIdDocuments() {

            if (Request.Files.AllKeys.Any())
            {
                var pic = System.Web.HttpContext.Current.Request.Files["Images"];
            }

            return Json(false);
        }


        /// <summary>
        /// Getting last messages from conversation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetLastMessages(string id) {
            using (var context = new ApplicationDbContext()) {
                var pm = new PersonalMessagesManager(context);

                return Json(pm.GetConversation(User.Identity.GetUserId(), id));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class PostNewMessageResponse {
            public bool Success { get; set; }

            public string UserId { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipientUsername"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public JsonResult PostNewMessage(string recipientUsername, string text) {
            var userData = UserManager.FindByName(recipientUsername);
            if (userData == null) {
                return Json(new PostNewMessageResponse {
                    Success = false
                });
            }

            using (var context = new ApplicationDbContext()) {
                if (new PersonalMessagesManager(context).SendMessage(User.Identity.GetUserId(), userData.Id.ToString(), text)) {
                    return Json(new PostNewMessageResponse {
                        Success = true,
                        UserId = userData.Id.ToString()
                    });
                }

                return Json(new PostNewMessageResponse {
                    Success = false
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult GetConversationRecipients() {
            using (var context = new ApplicationDbContext()) {
                return Json(new PersonalMessagesManager(context).GetLastConversations(User.Identity.GetUserId()));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public async Task<ActionResult> Deposit(string coinId){
            using (var context = new ApplicationDbContext()){
                DepositAddressesAssociations outData;
                var userId = User.Identity.GetUserId();
 

                var coinData = await CoinsManager.GetAsync(coinId);

                if (coinData.ShortName == "INR") {
                    return View("DepositINR", (object)coinId);
                }

                if (!await context.DepositAddressesAssociations.AnyAsync(addr => addr.CoinId == coinId && addr.UserId == userId))
                    outData = await DepositsManager.CreateAddressForDeposit(User.Identity.GetUserId(), coinId);
                else
                    outData = await context.DepositAddressesAssociations.OrderByDescending(order=>order.DateCreated).FirstAsync(addr => addr.CoinId == coinId && addr.UserId == userId);

                ViewBag.ConfirmationCount = coinData.ConfirmationCointForDeposit;

                ViewBag.PendingDeposits = await context.DepositsTransactions.Where(depo => depo.UserId == userId && !depo.Done).ToListAsync();

                return View(outData);
            }
        }



        [System.Web.Http.HttpPost]
        public async Task<ActionResult> DepositInr(IEnumerable<HttpPostedFileBase> files) {

            var userId = User.Identity.GetUserId();
            var picGuid = Guid.NewGuid();

            return View("DepositInrProcessing");
        }


        /// <summary>
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<JsonResult> CheckDeposit(string address){
            return Json(await DepositsManager.CheckDepositsOnAddress(address));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetNewDepositAddress(string coinId){
            var newData = await DepositsManager.CreateAddressForDeposit(User.Identity.GetUserId(), coinId);
            return Json(newData.Address);
        }

        /// <summary>
        /// </summary>
        /// <param name="coinId"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Withdraw(string coinId){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                var coinInfo = await context.CoinSystems.FirstAsync(coin => coin.Id.ToString() == coinId);
                var user = context.Users.First(_ => _.Id.ToString() == userId);

                if (coinInfo.ShortName == "INR")
                {
                    var currentBalance = BalancesManager.Get(userId, coinInfo.Id.ToString());

                    return View("WithdrawINR", new WithdrawINRRequest
                    {
                        CustomerName = user.Fullname,
                        Amount = currentBalance.Balance - coinInfo.WithdrawalFee
                    });
                }


                return View(new WithdrawViewModel{
                    CoinId = coinId,
                    WithdrawFee = coinInfo.WithdrawalFee,
                    AvailableUserAmount = BalancesManager.Get(User.Identity.GetUserId(),coinId).Balance
                });
            }
        }

        public class WithdrawINRRequest
        {
            public string CustomerName { get; set; }

            public string CustomerAccountNumber { get; set; }

            public string BankName { get; set; }

            public string IFSCCode { get; set; }

            public double Amount { get; set; }
        }

        public ActionResult WithdrawINR([FromBody]WithdrawINRRequest model)
        {

            using (var context = new ApplicationDbContext())
            {
                var userId = User.Identity.GetUserId();
                // Getting coin information
                var coinInfo = context.CoinSystems.First(coin => coin.ShortName == "INR");
                // Getting current balance
                var currentBalance = BalancesManager.Get(userId, coinInfo.Id.ToString());
                // Total Net Withdraw Amount
                var withdrawNetAmount = model.Amount - coinInfo.WithdrawalFee;

                if (withdrawNetAmount < 2500)
                {
                    ModelState.AddModelError("", "Minimum withdraw amount is 2500 Rupee");
                    return View(model);
                }

                // Check NetAmount more than zero
                if (withdrawNetAmount <= 0)
                {
                    ModelState.AddModelError("", "Net Amount can't be less zero");
                    return View(model);
                }

                if(model.CustomerName.Length < 5)
                {
                    ModelState.AddModelError("", "Wrong Customer Name");
                    return View(model);
                }

                if (model.CustomerAccountNumber.Length < 5)
                {
                    ModelState.AddModelError("", "Wrong Customer Account Number");
                    return View(model);
                }

                if (model.BankName.Length < 4)
                {
                    ModelState.AddModelError("", "Wrong Bank Name");
                    return View(model);
                }

                if (model.IFSCCode.Length < 3)
                {
                    ModelState.AddModelError("", "Wrong IFSCCode Name");
                    return View(model);
                }

                // Check enought Coins on balance
                if (currentBalance.Balance - withdrawNetAmount < 0)
                {
                    ModelState.AddModelError("", "Not enought coins");
                    return View(model);
                }

                // Checking total of coins now:
                // Initializing RPC connection
                var rpcManger = CoinsRpcManager.Init(coinInfo.Id.ToString());


                // Getting total coins amount,  "+ 0.00000001" fix for zero-division
                //var totalAmountOnHotWallet = (double) rpcManger.GetBalance() + 0.00000001;
                // Getting percentage of withdraw from Hot Wallet and compate it with 5.0
                //var percentage = withdrawNetAmount/totalAmountOnHotWallet*100 < 5.0;

                new BalancesManager(context).Withdraw(userId, coinInfo.Id.ToString(), withdrawNetAmount);

                WithdrawManager.AddInrWithdraw(userId, model.CustomerName, model.CustomerAccountNumber, model.BankName, model.IFSCCode, model.Amount);

                context.SaveChanges();

                return RedirectToAction("Balances");
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Withdrawals(){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                return View(await context.WithdrawRequests.Where(requests => requests.UserId == userId).ToListAsync());
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Deposits(){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                return View(await context.DepositsTransactions.Where(requests => requests.UserId == userId).ToListAsync());
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Orders(){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                return View(await context.Orders.Where(requests => requests.UserId == userId && !requests.Closed).ToListAsync());
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> TradeHistory(){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                var allOrders = await context.Orders.Where(requests => requests.UserId == userId && requests.Closed).ToListAsync();
                var allMarkets = await context.Markets.ToListAsync();
                foreach (var order in allOrders){
                    var marketData = allMarkets.First(mrkt => mrkt.Id.ToString() == order.MarketId);
                    allOrders.First(ord => ord.Id.ToString() == order.Id.ToString()).MarketId = marketData.PairName;
                }
                return View(allOrders);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> LoginHistory(){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                return View(await context.Logs.Where(__ => __.UserId == userId && (__.Type == Logs.LogType.LoggedIn || __.Type == Logs.LogType.FaliedLoggingIn)).OrderByDescending(_=>_.DateTime).ToListAsync());
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Vouchers(){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                return View(await context.Vouchers.Where(voucher => voucher.CreatorUserId == userId || voucher.RedeemerUserId == userId).ToListAsync());
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> CreateVoucher(){
            using (var context = new ApplicationDbContext()){
                ViewBag.CoinSystemsList = await context.CoinSystems.ToListAsync();

                return View();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> CancelVoucher(string id){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                var voucherData = context.Vouchers.First(voucher => voucher.CreatorUserId == userId && voucher.Id.ToString() == id);

                new BalancesManager(context).Deposit(userId, voucherData.CoinId, voucherData.Amount);

                context.Entry(voucherData).State = EntityState.Deleted;

                await context.SaveChangesAsync();

                return RedirectToAction("Vouchers");
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> RedeemVoucher(string id){
            return Json(await VouchersManager.Redeem(id, User.Identity.GetUserId()));
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> CreateVoucher(VouchCreatePageModel model){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                var balanceInfo = BalancesManager.Get(userId, model.CoinId);

                if (model.Amount < 0){
                    throw new Exception("Come on ;)");
                }


                if (balanceInfo.Balance < model.Amount){
                    ViewBag.CoinSystemsList = await CoinsManager.GetAllAsync();
                    ModelState.AddModelError("", "Voucher amount more than current balance");
                    return View(model);
                }

                new BalancesManager(context).Withdraw(userId, model.CoinId, model.Amount);

                await context.SaveChangesAsync();

                await VouchersManager.Create(userId, model.CoinId, model.Amount, model.ExpiryDays, model.Note);

                return RedirectToAction("Vouchers");
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> Withdraw(WithdrawViewModel model){
            using (var context = new ApplicationDbContext()){
                var userId = User.Identity.GetUserId();
                // Getting coin information
                var coinInfo = await context.CoinSystems.FirstAsync(coin => coin.Id.ToString() == model.CoinId);
                // Getting current balance
                var currentBalance = BalancesManager.Get(userId, model.CoinId);
                // Total Net Withdraw Amount
                var withdrawNetAmount = model.Amount - coinInfo.WithdrawalFee;

                // Check Pin Code Validity
                if (!await UserManager.ValidatePinCode(userId, model.PinCode)){
                    ModelState.AddModelError("", "Invalid Pin Code");
                    return View(model);
                }

                // Check NetAmount more than zero
                if (withdrawNetAmount < 0){
                    ModelState.AddModelError("", "Net Amount can't be less zero");
                    return View(model);
                }

                // Check enought Coins on balance
                if (currentBalance.Balance - withdrawNetAmount < 0){
                    ModelState.AddModelError("", "Not enought coins");
                    return View(model);
                }

                // Checking total of coins now:
                // Initializing RPC connection
                var rpcManger = CoinsRpcManager.Init(model.CoinId);

                // check address
                if (!rpcManger.ValidateAddress(model.Address).isvalid){
                    ModelState.AddModelError("", "Invalid Address provided");
                    return View(model);
                }

                // Getting total coins amount,  "+ 0.00000001" fix for zero-division
                //var totalAmountOnHotWallet = (double) rpcManger.GetBalance() + 0.00000001;
                // Getting percentage of withdraw from Hot Wallet and compate it with 5.0
                //var percentage = withdrawNetAmount/totalAmountOnHotWallet*100 < 5.0;

                new BalancesManager(context).Withdraw(userId, model.CoinId, withdrawNetAmount);

                // Add to DB new Withdraw request
                context.WithdrawRequests.Add(new WithdrawRequests{
                    Amount = withdrawNetAmount,
                    CoinId = model.CoinId,
                    DateCreated = DateTime.UtcNow,
                    UserId = userId,
                    Auto = true,
                    Address = model.Address,
                    Ip = Request.UserHostAddress,
                    Paid = false
                });

                // Save changes to DB
                await context.SaveChangesAsync();

                // Redirection
                return RedirectToAction("Withdrawals", "Manage");
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public class BalancesPageModel{

            /// <summary>
            /// 
            /// </summary>
            public class BalanceData{

                /// <summary>
                /// 
                /// </summary>
                public string CoinId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string CoinName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string Amount { get; set; }
            }

            /// <summary>
            /// 
            /// </summary>
            public List<BalanceData> Balances { get; set; } 
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Balances(){
            var userId = User.Identity.GetUserId();   
            using (var context = new ApplicationDbContext()){
                new BalancesManager(context).InitiateBalancesAndValidateNew(userId);

                var balances = new BalancesPageModel{
                    Balances = new List<BalancesPageModel.BalanceData>()
                };

                foreach (var balanceData in await context.Balances.Where(bal => bal.UserId == userId).OrderBy(bal => bal.Id).ToListAsync()){
                    balances.Balances.Add(new BalancesPageModel.BalanceData{
                         Amount = balanceData.Balance.ToString("N8"),
                         CoinId = balanceData.CoinId,
                         CoinName = CoinsManager.GetCoinNameById(balanceData.CoinId) 
                    });
                }

                // Alphabetical order
                balances.Balances = balances.Balances.OrderBy(bal => bal.CoinName).ToList();

                return View(balances);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task PinReset(){
            var userId = User.Identity.GetUserId();
            var newPin = await UsersManager.ResetPin(userId);

            await UserManager.SendEmailAsync(userId, $"Hi, {User.Identity.GetUserName()}, your new Pin Code", "Please, remember your new Pin Code: " + newPin);
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> DisableGoogleAuthenticator([FromBody] string pincode){
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            // Check Pin Code Validity
            if (!await UserManager.ValidatePinCode(user.Id, pincode)){
                return Json(new EnableGoogleAuthenticatorResult{
                    Result = false,
                    Error = "Invalid Pin Code"
                });
            }

            if (user != null){
                user.IsGoogleAuthenticatorEnabled = false;
                user.GoogleAuthenticatorSecretKey = null;

                await UserManager.UpdateAsync(user);

                await SignInAsync(user, false);

                await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            }

            return Json(new EnableGoogleAuthenticatorResult{
                Result = true
            });
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> EnableGoogleAuthenticator(){
            var secretKey = KeyGeneration.GenerateRandomKey(20);
            var userName = User.Identity.GetUserName();
            var barcodeUrl = KeyUrl.GetTotpUrl(secretKey, userName) + "&issuer=CoinPayIndia";

            var model = new GoogleAuthenticatorViewModel{
                SecretKey = Base32Encoder.Encode(secretKey),
                BarcodeUrl = HttpUtility.UrlEncode(barcodeUrl),
                Enabled = UserManager.GetTwoFactorEnabled(User.Identity.GetUserId())
            };
            return View(model);
        }

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> EnableGoogleAuthenticator(GoogleAuthenticatorViewModel model){
            if (!ModelState.IsValid)
                return Json(new EnableGoogleAuthenticatorResult{
                    Result = false,
                    Error = "Invalid Parameters"
                });

            var secretKey = Base32Encoder.Decode(model.SecretKey);

            long timeStepMatched;
            var otp = new Totp(secretKey);
            if (!otp.VerifyTotp(model.Code, out timeStepMatched, new VerificationWindow(2, 2)))
                return Json(new EnableGoogleAuthenticatorResult{
                    Result = false,
                    Error = "Wrong Code"
                });

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            user.IsGoogleAuthenticatorEnabled = true;
            user.GoogleAuthenticatorSecretKey = model.SecretKey;
            await UserManager.UpdateAsync(user);

            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);

            return Json(new EnableGoogleAuthenticatorResult{
                Result = true
            });
        }


        /// <summary>
        ///     POST: /Account/ChangePassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model){
            if (!ModelState.IsValid){
                return View("Index", model);
            }

            // Getting userId
            var userId = User.Identity.GetUserId();

            // Check Pin Code Validity
            if (!await UserManager.ValidatePinCode(userId, model.PinCode)){
                ModelState.AddModelError("", "Invalid Pin Code");
                return View("Index", model);
            }

            var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            if (result.Succeeded){
                var user = await UserManager.FindByIdAsync(userId);
                if (user != null){
                    await SignInAsync(user, false);
                }
                return RedirectToAction("Index", new{Message = ManageMessageId.ChangePasswordSuccess});
            }
            AddErrors(result);
            return View("Index", model);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        public enum ManageMessageId{
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager{
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent){
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties{IsPersistent = isPersistent}, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result){
            foreach (var error in result.Errors){
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword(){
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null){
                return user.PasswordHash != null;
            }
            return false;
        }
        #endregion

        public class EnableGoogleAuthenticatorResult{
            public bool Result { get; set; }
            public string Error { get; set; }
        }

        public class GoogleAuthenticatorViewModel{
            public string SecretKey { get; set; }
            public string BarcodeUrl { get; set; }
            public string Code { get; set; }

            public bool Enabled { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [StringLength(4, ErrorMessage = "The Pin Code must be {2} characters long.", MinimumLength = 4)]
            [Display(Name = "Confirm Disabling with Pin Code <a href='#' id='pin-restore-button'>Forgot?</a>")]
            public string PinCode { get; set; }
        }
    }
}