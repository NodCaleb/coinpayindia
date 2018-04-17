namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetInfoResponse{
        public decimal balance;
        public long blocks;
        public int connections;
        public decimal difficulty;
        public string errors;
        public long keypoololdest;
        public string keypoolsize;
        public decimal paytxfee;
        public int protocolversion;
        public string proxy;
        public bool testnet;
        public int unlocked_until;
        public int version;
        public int walletversion;
    }
}