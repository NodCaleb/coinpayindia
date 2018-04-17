#region

using System;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetWorkResponse{
        public string data;
        [Obsolete] public string hash1;
        [Obsolete] public string midstate;
        public string target;
    }
}