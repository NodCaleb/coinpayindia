#region

using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol{
    /// <summary>
    /// 
    /// </summary>
    public partial class BitcoinRPC{
        private readonly NetworkCredential _credentials;
        private readonly Uri _uri;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="credentials"></param>
        public BitcoinRPC(Uri uri, NetworkCredential credentials){
            _uri = uri;
            _credentials = credentials;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonRequest"></param>
        /// <returns></returns>
        protected string HttpCall(string jsonRequest){
            var request = (HttpWebRequest) WebRequest.Create(_uri);

            request.Method = "POST";
            request.ContentType = "application/json-rpc";
            request.Timeout = 5000;

            // always send auth to avoid 401 response
            var auth = _credentials.UserName + ":" + _credentials.Password;
            auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(auth), Base64FormattingOptions.None);
            request.Headers.Add("Authorization", "Basic " + auth);

            request.ContentLength = jsonRequest.Length;

            using (var sw = new StreamWriter(request.GetRequestStream())){
                sw.Write(jsonRequest);
            }

            try{
                using (var response = (HttpWebResponse) request.GetResponse())
                using (var sr = new StreamReader(response.GetResponseStream())){
                    return sr.ReadToEnd();
                }
            }
            catch (WebException wex){
                using (var response = (HttpWebResponse) wex.Response)
                using (var sr = new StreamReader(response.GetResponseStream())){
                    if (response.StatusCode != HttpStatusCode.InternalServerError){
                        throw;
                    }
                    return sr.ReadToEnd();
                }
            }
        }

        private T RpcCall<T>(RPCRequest rpcRequest){
            var jsonRequest = JsonConvert.SerializeObject(rpcRequest);

            var result = HttpCall(jsonRequest);

            try{
                var rpcResponse = JsonConvert.DeserializeObject<RPCResponse<T>>(result);

                if (rpcResponse.error != null) {
                    throw new BitcoinRpcException(rpcResponse.error);
                }
                return rpcResponse.result;
            }
            catch (Exception){
                throw new Exception("Problem with Coin Daemon at port: "+_uri.Port);
            } 
        }
    }
}