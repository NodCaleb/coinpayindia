#region

using System;

#endregion

namespace CryptoMarket.Source.Core.RPCProtocol.ResponseClasses{
    public class GetRawTransactionResponse{
        public string blockhash;
        public long blocktime;
        public int confirmations;
        public string hex;
        public int locktime;
        public string txid;
        public int version;
        public Input[] vin;
        public Output[] vout;

        public static implicit operator GetRawTransactionResponse(String s){
            return new GetRawTransactionResponse{hex = s};
        }

        public class Input{
            public ScriptSig scriptSig;
            public long sequence;
            public string txid;
            public int vout;

            public class ScriptSig{
                public string asm;
                public string hex;
            }
        }

        public class Output{
            public int n;
            public ScriptPubKey scriptPubKey;
            public decimal value;

            public class ScriptPubKey{
                public string[] addresses;
                public string asm;
                public string hex;
                public int reqSigs;
                public string type;
            }
        }
    }
}