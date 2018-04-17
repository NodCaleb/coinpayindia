#region

using System.Collections.Generic;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class ListSinceBlockResponse{
        public string lastblock;
        public IEnumerable<Transaction> transactions;

        public class Transaction{
            public string account;
            public string address;
            public decimal amount;
            public string blockhash;
            public int blockindex;
            public long blocktime;
            public string category;
            public long confirmations;
            public decimal fee;
            public long time;
            public long timereceived;
            public string txid;
        }
    }
}