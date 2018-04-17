#region

using System.Collections.Generic;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetBlockResponse{
        public string bits;
        public long confirmations;
        public decimal difficulty;
        public string hash;
        public long height;
        public string merkleroot;
        public string nextblockhash;
        public long nonce;
        public string previousblockhash;
        public int size;
        public long time;
        public IEnumerable<string> tx;
        public int version;
    }
}