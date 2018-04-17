#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class RPCRequest{
        private uint id = 1;
        private string jsonrpc = "2.0";
        private string method;

        [JsonProperty(PropertyName = "params", NullValueHandling = NullValueHandling.Ignore)] private IList<Object> requestParams;

        public RPCRequest(string method, IList<Object> requestParams = null, uint id = 1){
            this.method = method;
            this.requestParams = requestParams;
            this.id = id;

            String.IsNullOrEmpty(jsonrpc); // Suppress warning
        }
    }
}