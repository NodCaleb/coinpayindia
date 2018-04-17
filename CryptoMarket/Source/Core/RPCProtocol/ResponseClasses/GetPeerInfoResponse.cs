namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetPeerInfoResponse{
        public string addr;
        public int banscore;
        public long conntime;
        public bool inbound;
        public long lastrecv;
        public long lastsend;
        public long releasetime;
        public string services;
        public int startingheight;
        public string subver;
        public int version;
    }
}