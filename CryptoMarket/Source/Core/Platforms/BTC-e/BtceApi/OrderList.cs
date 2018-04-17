using System.Collections.Generic;
using System.Linq;
using CryptoMarket.Models.DB;
using Newtonsoft.Json.Linq;

namespace BtcE{
    public class OrderList{
        public Dictionary<int, Order> List { get; private set; }

        public static OrderList ReadFromJObject(JObject o){
            return new OrderList{
                List =
                    ((IDictionary<string, JToken>) o).ToDictionary(item => int.Parse(item.Key),
                        item =>
                            Order.ReadFromJObject(item.Value as JObject))
            };
        }

        public override string ToString(){
            return $"{List.Count} orders: buy={List.Count(o => o.Value.Type == TradeType.Buy)} sell={List.Count(o => o.Value.Type == TradeType.Sell)}";
        }
    }
}