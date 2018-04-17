// -----------------------------------------------------------------------
// <copyright file="BtceApiV3.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace BtcE{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public static class BtceApiV3{
        public static Dictionary<BtcePair, Depth> GetDepth(BtcePair[] pairlist, int limit = 150){
            return MakeRequest("depth", pairlist, x => Depth.ReadFromJObject(x as JObject),
                new Dictionary<string, string>{{"limit", limit.ToString()}});
        }

        public static Dictionary<BtcePair, Ticker> GetTicker(BtcePair[] pairlist){
            return MakeRequest("ticker", pairlist, x => Ticker.ReadFromJObject(x as JObject));
        }

        public static Dictionary<BtcePair, List<TradeInfo>> GetTrades(BtcePair[] pairlist, int limit = 150){
            Func<JContainer, List<TradeInfo>> tradeInfoListReader =
                x => x.OfType<JObject>().Select(TradeInfo.ReadFromJObjectV3).ToList();
            return MakeRequest("trades", pairlist, tradeInfoListReader,
                new Dictionary<string, string>{{"limit", limit.ToString()}});
        }

        private static string MakePairListString(BtcePair[] pairlist){
            return string.Join("-", pairlist.Select(x => BtcePairHelper.ToString(x)).ToArray());
        }

        private static Dictionary<BtcePair, T> MakeRequest<T>(string method, BtcePair[] pairlist,
            Func<JContainer, T> valueReader,
            Dictionary<string, string> args = null,
            bool ignoreInvalid = true){
            var queryresult = ignoreInvalid ? QueryIgnoreInvalid(method, pairlist, args) : Query(method, pairlist, args);
            var resobj = JObject.Parse(queryresult);

            if (resobj["success"] != null && resobj.Value<int>("success") == 0)
                throw new BtceException(resobj.Value<string>("error"));

            return ReadPairDict(resobj, valueReader);
        }

        private static string Query(string method, BtcePair[] pairlist, Dictionary<string, string> args = null){
            var pairliststr = MakePairListString(pairlist);
            var sb = new StringBuilder();
            sb.Append("https://wex.nz/api/3/");
            sb.Append(method);
            sb.Append("/");
            sb.Append(pairliststr);
            if (args != null && args.Count > 0){
                sb.Append("?");
                var arr = args.Select(x => $"{HttpUtility.UrlEncode(x.Key)}={HttpUtility.UrlEncode(x.Value)}").ToArray();
                sb.Append(string.Join("&", arr));
            }

            var queryStr = sb.ToString();
            return WebApi.Query(queryStr);
        }

        private static string QueryIgnoreInvalid(string method, BtcePair[] pairlist,
            Dictionary<string, string> args = null){
            var newargs = new Dictionary<string, string>{{"ignore_invalid", "1"}};
            if (args != null){
                foreach (var kvp in args){
                    newargs.Add(kvp.Key, kvp.Value);
                }
            }
            return Query(method, pairlist, newargs);
        }

        private static Dictionary<BtcePair, T> ReadPairDict<T>(JObject o, Func<JContainer, T> valueReader){
            return
                o.OfType<JProperty>()
                    .Select(
                        x =>
                            new KeyValuePair<BtcePair, T>(BtcePairHelper.FromString(x.Name), valueReader(x.Value as JContainer)))
                    .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}