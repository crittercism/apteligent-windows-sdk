using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
#if NETFX_CORE || WINDOWS_PHONE
using Windows.System.Threading;
#else
using System.Timers;
#endif // NETFX_CORE

namespace CrittercismSDK
{
    internal class TransactionReporter
    {
        #region Constants
        internal const int MSEC_PER_SEC = 1000;
        internal const int MAX_TRANSACTION_COUNT=50;
        const int ONE_HOUR = 3600 * MSEC_PER_SEC; // milliseconds
        #endregion

        #region Properties
        private static Object lockObject = new object();
        private static bool enabled = true;
        // Batch additional network requests for 20 seconds before sending TransactionReport .
        private static int interval = 20 * MSEC_PER_SEC; // milliseconds
        private static int defaultTimeout = ONE_HOUR; // milliseconds
        private static Dictionary<string,Object> thresholds = new Dictionary<string,Object>();

        internal static int Interval() {
            // Transaction batch reporting interval in milliseconds
            return interval;
        }

        internal static int DefaultTimeout() {
            // Transaction default timeout in milliseconds
            return defaultTimeout;
        }

        internal static bool IsForegrounded() {
            // TODO: NIY
            return true;
        }
        internal static void Background() {
            // TODO: NIY
        }
        internal static void Resume() {
            // TODO: NIY
        }
        #endregion

        #region Life Cycle
        internal static void Init() {
            lock (lockObject) {
                // Crittercism.Init calling TransactionReporter.Init should effectively make
                // lock lockObject here pointless, but no real harm doing so.
                // Initialize transactionsDictionary and TransactionsQueue
                transactionsDictionary = new Dictionary<string,Transaction>();
                TransactionsQueue = new SynchronizedQueue<Object[]>(new Queue<Object[]>());
            }
        }
        #endregion

        #region Persistence
        internal static void Save(Transaction transaction) {
            // Persist transaction to correct directory
            lock (lockObject) {
                switch (transaction.State()) {
                    case TransactionState.CREATED:
                        // Make visible via persistence API methods.
                        AddTransaction(transaction);
                        break;
                    case TransactionState.BEGUN:
                        // Nothing extra to do.
                        break;
                    case TransactionState.CRASHED:
                        RemoveTransaction(transaction);
                        break;
                    default:
                        // Final state
                        RemoveTransaction(transaction);
                        Enqueue(transaction);
                        break;
                }
            }
        }
        private static void AddTransaction(Transaction transaction) {
            lock (lockObject) {
                transactionsDictionary[transaction.Name()] = transaction;
            }
        }
        private static void RemoveTransaction(Transaction transaction) {
            lock (lockObject) {
                transactionsDictionary.Remove(transaction.Name());
            }
        }
        #endregion

        #region Transaction Dictionary
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
        internal static void Cancel(string name) {
            // Cancel a transaction as if it never was.
            // TODO: Kill transaction's timer if it exists.
            lock (lockObject) {
                if (transactionsDictionary.ContainsKey(name)) {
                    transactionsDictionary.Remove(name);
                }
            }
        }
        #endregion

        #region Normal Delivery
        // Different .NET frameworks get different timer's
#if NETFX_CORE || WINDOWS_PHONE
        private static ThreadPoolTimer timer=null;
        private static void OnTimerElapsed(ThreadPoolTimer timer) {
            lock (lockObject) {
                SendTransactionReport();
                timer=null;
            }
        }
#else
        private static Timer timer=null;
        private static void OnTimerElapsed(Object source, ElapsedEventArgs e) {
            lock (lockObject) {
                SendTransactionReport();
                timer=null;
            }
        }
#endif // NETFX_CORE
        private static SynchronizedQueue<Object[]> TransactionsQueue { get; set; }

        internal static void Enqueue(Transaction transaction) {
            lock (lockObject) {
                while (TransactionsQueue.Count>= MAX_TRANSACTION_COUNT) {
                    TransactionsQueue.Dequeue();
                };
                TransactionsQueue.Enqueue(transaction.ToArray());
#if NETFX_CORE || WINDOWS_PHONE
                if (timer==null) {
                    // Creates a single-use timer.
                    // https://msdn.microsoft.com/en-US/library/windows/apps/windows.system.threading.threadpooltimer.aspx
                    timer=ThreadPoolTimer.CreateTimer(
                        OnTimerElapsed,
                        TimeSpan.FromMilliseconds(Interval()));
                }
#else
                if (timer==null) {
                    // Generates an event after a set interval
                    // https://msdn.microsoft.com/en-us/library/system.timers.timer(v=vs.110).aspx
                    timer = new Timer(Interval());
                    timer.Elapsed += OnTimerElapsed;
                    // the Timer should raise the Elapsed event only once (false)
                    timer.AutoReset = false;        // fire once
                    timer.Enabled = true;           // Start the timer
                }
#endif // NETFX_CORE
            }
        }
        private static void SendTransactionReport() {
            if (TransactionsQueue.Count>0) {
                Object[] transactions = TransactionsQueue.ToArray();
                TransactionsQueue.Clear();
                Dictionary<string,object> appState = MessageReport.ComputeAppState();
                Breadcrumbs breadcrumbs = Crittercism.CurrentBreadcrumbs();
                // TODO: systemBreadcrumbs
                Object[] systemBreadcrumbs = new Object[] { };
                // TODO: endpoints
                Object[] endpoints = new Object[] { };
                TransactionReport transactionReport = new TransactionReport(
                    appState,
                    transactions,
                    breadcrumbs,
                    systemBreadcrumbs,
                    endpoints);
                Crittercism.AddMessageToQueue(transactionReport);
            }
        }
        #endregion

        #region Sampling Control
        internal static void Enable(int interval,int defaultTimeout,Dictionary<string,Object> thresholds) {
            ////////////////////////////////////////////////////////////////
            // Input:
            //     interval == milliseconds (millisecond == 10^-3 seconds)
            //     defaultTimeout == milliseconds (millisecond == 10^-3 seconds)
            //     thresholds == as received from platform AppLoad response
            //                   (Dictionary mapping string names to times in seconds)
            ////////////////////////////////////////////////////////////////
            lock (lockObject) {
                enabled = true;
                TransactionReporter.interval = interval;
                TransactionReporter.defaultTimeout = defaultTimeout;
                TransactionReporter.thresholds = thresholds;
            }
        }
        internal static void Disable() {
            lock (lockObject) {
                enabled = false;
            }
        }
        internal static int ClampTimeout(string name,int newTimeout) {
            // Clamp newTimeout according to Wire+Protocol doc
            // https://crittercism.atlassian.net/wiki/display/DEV/Wire+Protocol
            // details regarding txnConfig defaultTimeout and possible "timeout"
            // txnConfig transactions thresholds dictionaries.
            int answer = newTimeout;
            lock (lockObject) {
                if (thresholds.ContainsKey(name)) {
                    // thresholdTimeout in milliseconds
                    double thresholdTimeout = JsonUtils.StringToExtendedReal(thresholds[name]);
                    if ((thresholdTimeout > 0.0) && (answer > thresholdTimeout)) {
                        answer = (int)thresholdTimeout;
                    }
                } else {
                    ////////////////////////////////////////////////////////////////
                    // Don't go over global "defaultTimeout" ticks
                    ////////////////////////////////////////////////////////////////
                    answer = Math.Min(answer,defaultTimeout);
                }
            }
            return answer;
        }
        #endregion
    }
}
