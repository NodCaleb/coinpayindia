namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class ListReceivedByAddressResponse{
        public string account;
        public string address;
        public decimal amount;
        public long confirmations;
    }
}