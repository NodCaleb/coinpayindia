#region

using System;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol{
    internal class BitcoinRpcException : Exception{
        public BitcoinRpcException(RPCError rpcError)
            : base(rpcError.message){
            Error = rpcError;
        }

        public BitcoinRpcException(RPCError rpcError, Exception innerException)
            : base(rpcError.message, innerException){
            Error = rpcError;
        }

        public RPCError Error { get; private set; }
    }
}