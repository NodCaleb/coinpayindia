namespace CryptoMarket.Source.Core.RPCProtocol{
    public class RPCResponse<T>{
        public RPCError error;
        public uint id;
        public T result;
    }
}