#region

using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;

#endregion

namespace CryptoMarket.Source.Managers{
    /// <summary>
    /// </summary>
    public static class UsersManager{
        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetUserObfuscatedEmail(string userId){
            using (var context = new ApplicationDbContext()){
                var nonObfuscated = context.Users.First(user => user.Id.ToString() == userId).Email;
                var split = nonObfuscated.Split('@');
                var firstPart = split[0].Substring(2, split[0].Length - 3);
                var asterisks = string.Empty;
                for (var i = 0; i < firstPart.Length; i++){
                    asterisks += "*";
                }
                var obfuscatedFirstPart = split[0].Replace(firstPart, asterisks);
                return $"{obfuscatedFirstPart}@{split[1]}";
            }
        }

        public static VerificationManager.VerificationLevel GetVerificationLevel(string userId) {
            using (var context = new ApplicationDbContext())
            {
                var userInfo = context.Users.First(user => user.Id.ToString() == userId);
                return userInfo.VerificationLevel;
            }
        }


        public static ApplicationUser Get(string userId)
        {
            using (var context = new ApplicationDbContext())
            {
                var userInfo = context.Users.First(user => user.Id.ToString() == userId);
                return userInfo;
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsUserLocked(string userId){
            using (var context = new ApplicationDbContext()){
                var userInfo = context.Users.First(user => user.Id.ToString() == userId);
                return userInfo.LockoutEnabled;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<string> ResetPin(string userId){
            using (var context = new ApplicationDbContext()){
                var userData = await context.Users.FirstAsync(user => user.Id.ToString() == userId);
                userData.PinCode = new Random().Next(1000, 9999).ToString("D");

                context.Entry(userData).State = EntityState.Modified;

                await context.SaveChangesAsync();

                return userData.PinCode;
            }
        }
    }
}