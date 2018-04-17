#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;
using Newtonsoft.Json;

#endregion

namespace CryptoMarket.Source.Core.CustomCoinsProtocols{
    /// <summary>
    ///     Wrapper for NXT Coin Protocol
    /// </summary>
    public static class NxtCoinProtocol{
        private const string APIEndpoint = "http://127.0.0.1:7876/nxt";
        private static string GetSalt => $"NXT-SALT={DateTime.UtcNow.Millisecond}{Guid.NewGuid()}".ToMD5();

        /// <summary>
        ///     Try to init access to NXT API
        /// </summary>
        /// <returns></returns>
        public static bool TryInit(){
            try{
                return GenerateNewAccount(Guid.NewGuid().ToString()).AccountId != null;
            }
            catch{
                // API Not online
                return false;
            }
        }

        public static bool IsAccountValid(string accountId){
            // Obtain parameters for request
            var parameters = new NameValueCollection{
                {"requestType", "getAccount"}, // Basic Method Calling, don't change
                {"account", accountId}
            };

            var dataFromApi = ApiHttpRequest<AccountInfo>(parameters);

            return dataFromApi.PublicKey != null;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static double GetWalletBalance(){
            return 69;
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static NewAccountGenerateResponse GenerateNewAccount(string userId){
            again: // Sorry for goto-again...
            // Obtain parameters for request
            var privateKey = $"{GetSalt}-{userId.ToMD5()}";
            var parameters = new NameValueCollection{
                {"requestType", "getAccountId"}, // Basic Method Calling, don't change
                {"secretPhrase", privateKey}
            };

            var dataFromApi = ApiHttpRequest<NewAccountGenerateResponse>(parameters);

            // check collision
            if (!VerifyCollision(dataFromApi.AccountId))
                goto again;

            using (var context = new ApplicationDbContext()){
                context.NxtCoinPrivateKeys.Add(new NxtCoinPrivateKeys{
                    AccountId = dataFromApi.AccountId,
                    PrivateKey = privateKey
                });

                context.SaveChanges();
            }

            return dataFromApi;
        }

        /// <summary>
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static AccountTransactions GetAccountTransactions(string accountId){
            var parameters = new NameValueCollection{
                {"requestType", "getAccountTransactionIds"}, // Basic Method Calling, don't change
                {"account", accountId}
            };

            var dataFromApi = ApiHttpRequest<AccountTransactions>(parameters);

            // Protect from NULL-parsing result
            if (dataFromApi.TransactionIds == null){
                return new AccountTransactions{
                    TransactionIds = new List<string>()
                };
            }

            return dataFromApi;
        }


        /// <summary>
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public static TransactionInfo GetTransactionInfo(string transactionId){
            var parameters = new NameValueCollection{
                {"requestType", "getTransaction"}, // Basic Method Calling, don't change
                {"transaction", transactionId}
            };

            var dataFromApi = ApiHttpRequest<TransactionInfo>(parameters);

            return dataFromApi;
        }

        private static bool VerifyCollision(string nxtAccountId){
            try{
                var parameters = new NameValueCollection{
                    {"requestType", "getAccountPublicKey"}, // Basic Method Calling, don't change
                    {"account", nxtAccountId}
                };
                return ApiHttpRequest<ErrorResponse>(parameters).Code == 5;
            }
            catch{
                return false;
            }
        }

        private static T ApiHttpRequest<T>(NameValueCollection parameters){
            var request = (HttpWebRequest) WebRequest.Create(string.Format("{0}{1}", APIEndpoint, ToQueryString(parameters)));

            request.Method = "POST";

            string responseString;

            try{
                using (var response = (HttpWebResponse) request.GetResponse())
                using (var sr = new StreamReader(response.GetResponseStream())){
                    responseString = sr.ReadToEnd();
                }
            }
            catch (WebException wex){
                using (var response = (HttpWebResponse) wex.Response)
                using (var sr = new StreamReader(response.GetResponseStream())){
                    if (response.StatusCode != HttpStatusCode.InternalServerError){
                        throw;
                    }
                    responseString = sr.ReadToEnd();
                }
            }

            return JsonConvert.DeserializeObject<T>(@responseString);
        }

        private static string ToQueryString(NameValueCollection nvc){
            var array = (from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value))).ToArray();
            return "?" + string.Join("&", array);
        }

        public class AccountInfo{
            /// <summary>
            /// </summary>
            [JsonProperty("publicKey")]
            public string PublicKey { get; set; }
        }

        /// <summary>
        /// </summary>
        public class AccountTransactions{
            /// <summary>
            /// </summary>
            [JsonProperty("transactionIds")]
            public List<string> TransactionIds { get; set; }
        }

        /// <summary>
        /// </summary>
        public class ErrorResponse{
            /// <summary>
            /// </summary>
            [JsonProperty("errorCode")]
            public int Code { get; set; }

            /// <summary>
            /// </summary>
            [JsonProperty("errorDescription")]
            public string Description { get; set; }
        }

        /// <summary>
        /// </summary>
        public class NewAccountGenerateResponse{
            /// <summary>
            /// </summary>
            [JsonProperty("accountId")]
            public string AccountId { get; set; }

            /// <summary>
            /// </summary>
            [JsonProperty("accountRS")]
            public string AccountRs { get; set; }
        }

        /// <summary>
        /// </summary>
        public class TransactionInfo{
            /// <summary>
            /// </summary>
            [JsonProperty("confirmations")]
            public int ConfirmationCount { get; set; }

            /// <summary>
            /// </summary>
            [JsonProperty("recipient")]
            public string RecipientId { get; set; }

            /// <summary>
            /// </summary>
            [JsonProperty("amountNQT")]
            public double Amount { get; set; }
        }
    }
}