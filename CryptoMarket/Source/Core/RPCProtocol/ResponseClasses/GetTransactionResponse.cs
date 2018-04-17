#region

using System.Collections.Generic;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetTransactionResponse{
        public decimal amount;
        public string blockhash;
        public int blockindex;
        public long confirmations;
        public IEnumerable<Details> details;
        public decimal fee;
        public long time;
        public long timereceived;
        public string txid;

        public class Details{
            public string account;
            public string address;
            public decimal amount;
            public string category;
            public decimal fee;
        }
    }
}