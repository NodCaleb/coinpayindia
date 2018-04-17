#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CryptoMarket.Models;
using CryptoMarket.Models.DB;

#endregion

namespace CryptoMarket.Source.Managers {
    public static class LogManager {
        /// <summary>
        ///     Write new log entry to DB
        /// </summary>
        /// <param name="type">Log Type</param>
        /// <param name="text">Log Text</param>
        /// <param name="eventCallerUserId">User Id</param>
        /// <param name="eventCallerUserIp">User Ip(if has)</param>
        /// <returns></returns>
        public static async Task WriteAsync(Logs.LogType type, string text, string eventCallerUserId, string eventCallerUserIp) {
            using (var context = new ApplicationDbContext()) {
                context.Logs.Add(new Logs {
                    Type = type,
                    Text = text,
                    Ip = eventCallerUserIp,
                    UserId = eventCallerUserId,
                    DateTime = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }
        }

        public static IEnumerable<Logs> GetAllUserLogs(string userId) {
            using (var context = new ApplicationDbContext()) {
                return context.Logs.Where(log => log.UserId == userId).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public static void WriteToEventLog(string text) {
            var sSource = "Exchange Software";
            var sLog = "Application";


            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);

            EventLog.WriteEntry(sSource, text);
        }
    }
}