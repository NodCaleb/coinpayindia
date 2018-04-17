namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetTxOutResponse{
        public string bestblock;
        public bool coinbase;
        public int confirmations;
        public ScriptPubKey scriptPubKey;
        public decimal value;
        public int version;

        public class ScriptPubKey{
            public string[] addresses;
            public string asm;
            public string hex;
            public int reqSigs;
            public string type;
        }
    }
}