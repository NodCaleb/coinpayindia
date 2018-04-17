namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetAddedNodeInfoResponse{
        public string addednode;
        public Addresses[] addresses;
        public bool connected;

        public class Addresses{
            public string address;
            public bool connected;
        }
    }
}