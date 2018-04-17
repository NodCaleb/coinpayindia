using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BtcE{
    public class Funds{
        private Dictionary<string, decimal> _allValues;

        public decimal Btc => _allValues["btc"];

        public decimal Eur => _allValues["eur"];

        public decimal Ftc => _allValues["ftc"];

        public decimal Ltc => _allValues["ltc"];

        public decimal Nmc => _allValues["nmc"];

        public decimal Nvc => _allValues["nvc"];

        public decimal Ppc => _allValues["ppc"];

        public decimal Rur => _allValues["rur"];

        public decimal Trc => _allValues["trc"];

        public decimal Usd => _allValues["usd"];

        public static Funds ReadFromJObject(JObject o){
            if (o == null)
                return null;
            return new Funds{
                _allValues = ((IDictionary<string, JToken>) o).ToDictionary(a => a.Key, a => (decimal) a.Value)
            };
        }

        public decimal GetFund(BtceCurrency cur){
            return (GetFund(cur.ToString()));
        }

        public decimal GetFund(string cur){
            return (_allValues[cur.ToLowerInvariant()]);
        }

        public string[] GetFundValues(){
            return _allValues.Keys.ToArray();
        }

        public override string ToString(){
            return $"btc:{Btc} ltc:{Ltc} ppc:{Ppc} nmc:{Nmc} trc:{Trc}";
        }
    }
}