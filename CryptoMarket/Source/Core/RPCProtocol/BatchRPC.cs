#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol{
    public partial class BatchRpc : BitcoinRPC{
        private readonly List<RPCRequest> _requests;
        private uint _lastID = 1;
        private Dictionary<uint, RPCResponse<JObject>> _responses;

        public BatchRpc(Uri uri, NetworkCredential credentials) : base(uri, credentials){
            _requests = new List<RPCRequest>();
        }

        private uint NewID{
            get { return ++_lastID; }
        }

        public void DoRequest(){
            var jsonRequest = JsonConvert.SerializeObject(_requests);

            var result = HttpCall(jsonRequest);

            var responseList = JsonConvert.DeserializeObject<IEnumerable<RPCResponse<JObject>>>(result);

            _responses = responseList.ToDictionary(x => x.id);

            _requests.Clear();
        }

        public T GetResult<T>(uint ID){
            var r = _responses[ID];
            if (r.error != null){
                throw new BitcoinRpcException(r.error);
            }
            return JsonConvert.DeserializeObject<T>(r.result.ToString());
        }
    }
}