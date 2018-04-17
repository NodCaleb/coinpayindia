using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using CryptoMarket.Models;
using Microsoft.AspNet.Identity;
using Twilio;
using System.Web;
using System.Net;
using System.IO;
using System.Text;

namespace CryptoMarket.Source.Managers {
    /// <summary>
    /// KYC system
    /// </summary>
    public class VerificationManager {

        private readonly ApplicationDbContext _context;

        private readonly ApplicationUserManager _userManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        public VerificationManager(ApplicationDbContext context, ApplicationUserManager userManager) {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static double GetWithdrawLimitByLevel(VerificationLevel level) {
            switch (level) {
                case VerificationLevel.Email:
                    return 2500;

                case VerificationLevel.Phone:
                    return 5000;

                case VerificationLevel.Passport:
                    return 25000;

                case VerificationLevel.Corporate:
                    return 1000000;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        /// <summary>
        /// Getting user limit for today
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public double GetLimit(string userId) {
            return !_context.WithdrawLimits.Any(_ => _.UserId == userId && _.Date == DateTime.UtcNow.Date) ? 
                GetWithdrawLimitByLevel(_userManager.FindById(userId).VerificationLevel) : 
                GetWithdrawLimitByLevel(_userManager.FindById(userId).VerificationLevel) - _context.WithdrawLimits.First(_ => _.UserId == userId && _.Date == DateTime.UtcNow.Date).USDLimitUsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public VerificationLevel GetLevel(string userId) {
            return _userManager.FindById(userId).VerificationLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<ApplicationUser> GetUsersForVerification() {
            using(var context = new ApplicationDbContext())
            return context.Users.Where(_ => _.VerificationLevel == VerificationLevel.AwaitForPassportValidation).ToList();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="picGuid"></param>
        /// <param name="userId"></param>
        public void InitiateIdVerification(string picGuid, string userId) {
            var userInfo = _userManager.FindById(userId);
            userInfo.VerificationDocumentImageGuid = picGuid;
            userInfo.VerificationLevel = VerificationLevel.AwaitForPassportValidation;
            
            _userManager.Update(userInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="vlevel"></param>
        /// <returns></returns>
        public bool SetVerificationLevel(string userId, VerificationLevel vlevel) {
            var userInfo = _userManager.FindById(userId);
            userInfo.VerificationLevel = vlevel;
            return _userManager.Update(userInfo).Succeeded;
        }

        public bool SetDocumentVerification(string userId, string fullName) {
            var userInfo = _userManager.FindById(userId);
            userInfo.VerificationLevel = VerificationLevel.Passport;
            userInfo.Fullname = fullName;
            return _userManager.Update(userInfo).Succeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public bool SendVerificationCodeToPhone(string userId, string phoneNumber) {
            var userInfo = _userManager.FindById(userId);
            var code = new Random().Next(10000, 99999);

            userInfo.PhoneNumber = phoneNumber;
            userInfo.PhoneNumberVerificationCode = code.ToString();

            return SendSMS(phoneNumber, code.ToString()) && _userManager.Update(userInfo).Succeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public bool CheckPhoneNumberVerification(string userId, string userCode) {
            var userInfo = _userManager.FindById(userId);

            if (userInfo.PhoneNumberVerificationCode == userCode) {
                userInfo.PhoneNumberConfirmed = true;
                userInfo.VerificationLevel = VerificationLevel.Phone;  
                      
                return _userManager.Update(userInfo).Succeeded;
            }

            return false;
        }



        public static bool SendSMS(string phoneNo, string message) {

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
                return false;
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

            LogManager.WriteToEventLog(result);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public enum VerificationLevel {
            /// <summary>
            /// 
            /// </summary>
            Email,

            /// <summary>
            /// 
            /// </summary>
            Phone,

            /// <summary>
            /// 
            /// </summary>
            Passport,

            /// <summary>
            /// 
            /// </summary>
            Corporate,

            AwaitForPassportValidation
        }

        /// <summary>
        /// 
        /// </summary>
        public class DayUsedWithdrawLimit {
            /// <summary>
            /// 
            /// </summary>
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public Guid Id { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string UserId { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double USDLimitUsed { get; set; }
        }
    }
}