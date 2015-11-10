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
        internal const long MSEC_PER_SEC = 1000;
        internal const int MAX_TRANSACTION_COUNT=50;
        const long ONE_HOUR = 3600 * TimeSpan.TicksPerSecond;
        #endregion

        #region Properties
        private static Object lockObject = new object();
        private static bool enabled = true;
        // Batch additional network requests for 20 seconds before sending TransactionReport .
        private static long interval = 20 * MSEC_PER_SEC; // ms
        private static long defaultTimeout = ONE_HOUR; // ticks
        private static Dictionary<string,Object> thresholds = new Dictionary<string,Object>();

        internal static long Interval() {
            return interval;
        }

        internal static long DefaultTimeout() {
            return defaultTimeout;
        }

        internal static bool IsForegrounded() {
            // TODO: NIY
            return true;
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
                // TODO: "Aborted" transactions?
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
        static void Enable(long interval,long defaultTimeout,Dictionary<string,Object> thresholds) {
            ////////////////////////////////////////////////////////////////
            // Input:
            //     interval == ms (ms == 10^-3 seconds)
            //     defaultTimeout == ticks (tick == 10^-7 seconds)
            //     thresholds == as received from platform AppLoad response
            //                   (Dictionary mapping string names to times in seconds)
            // TODO: ms, ticks, seconds all in one function?  Is that best?
            ////////////////////////////////////////////////////////////////
            lock (lockObject) {
                enabled = true;
                TransactionReporter.interval = interval;
                TransactionReporter.defaultTimeout = defaultTimeout;
                TransactionReporter.thresholds = thresholds;
            }
        }
        static void Disable() {
            lock (lockObject) {
                enabled = false;
            }
        }
        static long ClampTimeout(string name,long newTimeout) {
            long answer = newTimeout;
            lock (lockObject) {
                if (thresholds.ContainsKey(name)) {
                    const int TICKS_PER_MS = 10000;
                    // thresholdTimeout in ticks
                    double thresholdTimeout = TICKS_PER_MS*JsonUtils.StringToExtendedReal(thresholds[name]);
                    if ((thresholdTimeout > 0.0) && (answer > thresholdTimeout)) {
                        answer = (long)thresholdTimeout;
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
