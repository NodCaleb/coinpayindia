namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class ValidateAddressResponse{
        public string account;
        public string address;
        public bool iscompressed;
        public bool ismine;
        public bool isscript;
        public bool isvalid;
        public string pubkey;
    }
}