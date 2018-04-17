namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class ListTransactionsResponse{
        public string account;
        public string address;
        public decimal amount;
        public string blockhash;
        public int blockindex;
        public long blocktime;
        public string category;
        public string comment;
        public long confirmations;
        public decimal fee;
        public string otheraccount;
        public long time;
        public long timereceived;
        public string txid;
    }
}