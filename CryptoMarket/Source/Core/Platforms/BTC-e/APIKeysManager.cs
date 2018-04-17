using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using CryptoMarket.Models;


namespace CryptoMarket.Source.Core.Platforms.BTC_e{
    public class APIKeysManager{
        public class ApiKeyData{

            public string Key { get; set; }

            public string Secret { get; set; }
        }

        private static readonly Dictionary<string, ApiKeyData> KeyCache = new Dictionary<string, ApiKeyData>();

        /// <summary>
        /// Remove user's api credentials from cache
        /// </summary>
        /// <param name="userId"></param>
        public static void DropCache(string userId){
            if (KeyCache.ContainsKey(userId))
                KeyCache.Remove(userId);
        }

        /// <summary>
        /// Getting stored in memory API Credentials
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns></returns>
        public static ApiKeyData GetApiCredentials(string userId){
            if (KeyCache.ContainsKey(userId))
                return KeyCache[userId];

            using (var context = new ApplicationDbContext()){
                var userApi = context.Users.AsNoTracking().First(_ => _.Id == userId);

                var apiData = new ApiKeyData{
                    Key = AES256Helper.Decrypt(userApi.BtceKey),
                    Secret = AES256Helper.Decrypt(userApi.BtceSecret)

                };

                KeyCache.Add(userId, apiData);

                return apiData;
            }
        }

        /// <summary>
        /// Setting Api Credentials into DB and load to memory
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        public static void SetApiCredentials(string userId, string key, string secret){
            DropCache(userId);

            using (var context = new ApplicationDbContext()){
                var userApi = context.Users.First(_ => _.Id == userId);

                userApi.BtceKey = AES256Helper.Encrypt(key);
                userApi.BtceSecret = AES256Helper.Encrypt(secret);

                context.Entry(userApi).State = EntityState.Modified;

                if (context.SaveChanges() > 0){
                    KeyCache.Add(userId, new ApiKeyData{
                        Key = key,
                        Secret = secret
                    });
                }
                else{
                    throw new DbUpdateException();
                }
            }
        }

    }
}