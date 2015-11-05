using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal class TransactionReporter
    {
        #region Constants
        internal const double MSEC_PER_SEC=1000.0;
        internal const int MAX_TRANSACTION_COUNT=50;
        const long ONE_HOUR = 3600 * TimeSpan.TicksPerSecond;
        #endregion

        #region Properties
        private static long interval = 20 * TimeSpan.TicksPerSecond;
        private static long defaultTimeout = ONE_HOUR;
        #endregion

        internal static long Interval() {
            // TODO: NIY
            return interval;
        }

        internal static long DefaultTimeout() {
            // TODO: NIY
            return defaultTimeout;
        }

        internal static bool IsForegrounded() {
            // TODO: NIY
            return true;
        }

        private static Object lockObject = new object();
        private static Dictionary<string,Transaction> transactionsDictionary;
        internal static int TransactionCount() {
            int answer = 0;
            lock (lockObject) {
                answer=transactionsDictionary.Count;
            }
            return answer;
        }
        internal static Transaction[] AllTransactions() {
            Transaction[] answer = null;
            lock (lockObject) {
                List<Transaction> list = new List<Transaction>();
                foreach (Transaction transaction in transactionsDictionary.Values) {
                    list.Add(transaction);
                }
                answer = list.ToArray();
            }
            return answer;
        }
        internal static Transaction TransactionForName(string name) {
            Transaction answer = null;
            lock (lockObject) {
                if (transactionsDictionary.ContainsKey(name)) {
                    answer = transactionsDictionary[name];
                }
            }
            return answer;
        }

        internal static void Init() {
            lock (lockObject) {
                // Crittercism.Init calling TransactionReporter.Init should effectively make
                // lock lockObject here pointless, but no real harm doing so.
                transactionsDictionary = new Dictionary<string,Transaction>();
            }
        }
    }
}
