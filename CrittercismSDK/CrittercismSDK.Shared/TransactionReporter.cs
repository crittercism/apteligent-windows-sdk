using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{
    internal class TransactionReporter
    {
        private static Object lockObject = new object();
        private static Dictionary<string,Transaction> transactionsDictionary;
        internal static int transactionCount() {
            int answer = 0;
            lock (lockObject) {
                answer=transactionsDictionary.Count;
            }
            return answer;
        }
        internal static Transaction transactionForName(string name) {
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
