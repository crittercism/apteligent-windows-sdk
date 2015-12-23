using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
#if NETFX_CORE || WINDOWS_PHONE
using Windows.System.Threading;
#else
using System.Timers;
#endif // NETFX_CORE
#if WINDOWS_PHONE
using Microsoft.Phone.Info;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Microsoft.Phone.Net.NetworkInformation;
#endif

namespace CrittercismSDK {
    internal class TransactionReporter {
        #region Constants
        internal const int MSEC_PER_SEC = 1000;
        internal const int MAX_TRANSACTION_COUNT = 50;
        private const int ONE_MINUTE = 60 * MSEC_PER_SEC; // milliseconds
        private const int ONE_HOUR = 3600 * MSEC_PER_SEC; // milliseconds
        #endregion

        #region Properties
        private static Object lockObject = new object();
        private static SynchronizedQueue<Transaction> TransactionsQueue { get; set; }
        private static bool enabled = true;
        // Batch additional network requests for 20 seconds before sending TransactionReport .
        private static int interval = 20 * MSEC_PER_SEC; // milliseconds
        private static int defaultTimeout = ONE_HOUR; // milliseconds
        // private static int defaultTimeout = ONE_MINUTE; // milliseconds
        private static JObject thresholds = null;

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
            lock (lockObject) {
                long backgroundTime = DateTime.UtcNow.Ticks;
                foreach (Transaction transaction in transactionsDictionary.Values) {
                    transaction.Background(backgroundTime);
                };
                RemoveTimer();
            }
        }
        internal static void Resume() {
            lock (lockObject) {
                long foregroundTime = DateTime.UtcNow.Ticks;
                foreach (Transaction transaction in transactionsDictionary.Values) {
                    transaction.Foreground(foregroundTime);
                };
                // TODO: Timer.  Entire Resume method needs more work.
            }
        }
        #endregion

        #region Multithreading Remarks
        ////////////////////////////////////////////////////////////////
        // MULTITHREADING Transaction AND TransactionReporter REMARKS
        // * lock "Lock Ordering"
        //    The deadlock prevention technique of establishing a global ordering
        //    on resources which may be locked is known as "Lock Ordering"
        //    Lock ordering is a simple yet effective deadlock prevention mechanism.
        //    http://tutorials.jenkov.com/java-concurrency/deadlock-prevention.html#ordering
        // Transaction code sometimes needs to lock both a transaction and
        // the reporter simultaneously.  Our lock deadlock prevention
        // policy is to always obtain lock on transaction first and lock
        // on reporter second whenever this is necessary.  Effectively:
        //    lock (transaction) {
        //      lock (TransactionReporter.lockObject) {
        //        ...
        //      }
        //    }
        // though possibly spread out on the stack trace, not necessarily
        // all in one function or class.
        // * Changing properties of transactions is done synchronously.
        ////////////////////////////////////////////////////////////////
        #endregion

        #region Life Cycle
        internal static void Init() {
            lock (lockObject) {
                // Crittercism.Init calling TransactionReporter.Init should effectively make
                // lock lockObject here pointless, but no real harm doing so.
                SettingsChange();
                // Initialize transactionsDictionary and TransactionsQueue
                transactionsDictionary = new Dictionary<string,Transaction>();
                TransactionsQueue = new SynchronizedQueue<Transaction>(new Queue<Transaction>());
            }
        }
        internal static void Shutdown() {
            lock (lockObject) {
                // Crittercism.Shutdown calls TransactionReporter.Shutdown
                Background();
            }
        }
        #endregion

        #region Persistence
        internal static void Save(Transaction transaction) {
            // Persist transaction to correct directory
            lock (lockObject) {
                switch (transaction.State()) {
                    case TransactionState.CANCELLED:
                        CancelTransaction(transaction);
                        break;
                    case TransactionState.CREATED:
                        // Make visible via persistence API methods.
                        AddTransaction(transaction);
                        break;
                    case TransactionState.BEGUN:
                        // Nothing extra to do.
                        break;
                    case TransactionState.CRASHED:
                        CancelTransaction(transaction);
                        break;
                    default:
                        // Final state
                        CancelTransaction(transaction);
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
        private static void CancelTransaction(Transaction transaction) {
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
                answer = transactionsDictionary.Count;
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
        #endregion

        #region Timing
        // Different .NET frameworks get different timer's
#if NETFX_CORE || WINDOWS_PHONE
        private static ThreadPoolTimer timer = null;
        private static void OnTimerElapsed(ThreadPoolTimer timer) {
            lock (lockObject) {
                SendTransactionReport();
                timer = null;
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
        private static void RemoveTimer() {
            // Call if we don't need the timer anymore.
#if NETFX_CORE || WINDOWS_PHONE
            if (timer != null) {
                timer.Cancel();
                timer = null;
            }
#else
            if (timer!=null) {
                timer.Stop();
                timer = null;
            }
#endif // NETFX_CORE
        }
        #endregion

        #region Reporting
        internal static void Enqueue(Transaction transaction) {
            lock (lockObject) {
                if (enabled) {
                    while (TransactionsQueue.Count >= MAX_TRANSACTION_COUNT) {
                        TransactionsQueue.Dequeue();
                    };
                    TransactionsQueue.Enqueue(transaction);
#if NETFX_CORE || WINDOWS_PHONE
                    if (timer == null) {
                        // Creates a single-use timer.
                        // https://msdn.microsoft.com/en-US/library/windows/apps/windows.system.threading.threadpooltimer.aspx
                        timer = ThreadPoolTimer.CreateTimer(
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
        }
        private static long BeginTime(List<Transaction> transactions) {
            // Earliest BeginTime amongst these transactions
            long answer = long.MaxValue;
            foreach (Transaction transaction in transactions) {
                answer = Math.Min(answer,transaction.BeginTime());
            }
            return answer;
        }
        private static long EndTime(List<Transaction> transactions) {
            // Latest EndTime amongst these transactions
            long answer = long.MinValue;
            foreach (Transaction transaction in transactions) {
                answer = Math.Max(answer,transaction.EndTime());
            }
            return answer;
        }

        private static void SendTransactionReport() {
            if (TransactionsQueue.Count > 0) {
                List<Transaction> transactions = TransactionsQueue.ToList();
                TransactionsQueue.Clear();
                long beginTime = BeginTime(transactions);
                long endTime = EndTime(transactions);
                Dictionary<string,object> appState = MessageReport.ComputeAppState();
                List<UserBreadcrumb> breadcrumbs = Breadcrumbs.ExtractUserBreadcrumbs(beginTime,endTime);
                List<Breadcrumb> systemBreadcrumbs = Breadcrumbs.SystemBreadcrumbs().RecentBreadcrumbs(beginTime,endTime);
                List<Endpoint> endpoints = Breadcrumbs.ExtractEndpoints(beginTime,endTime);
                TransactionReport transactionReport = new TransactionReport(
                    appState,
                    transactions,
                    breadcrumbs,
                    systemBreadcrumbs,
                    endpoints);
                Crittercism.AddMessageToQueue(transactionReport);
            }
        }
        internal static List<Transaction> CrashTransactions() {
            // Remove BEGUN Transaction's to CRASHED state.
            // The code takes care to avoid deadlocks by not locking
            // the reporter and then locking a Transaction (wrong order).
            Transaction[] allTransactions = AllTransactions();
            // Compute crashed Transaction's .
            List<Transaction> answer = new List<Transaction>();
            foreach (Transaction transaction in allTransactions) {
                // Request transaction to Crash.
                transaction.Crash();
                if (transaction.State() == TransactionState.CRASHED) {
                    // The transaction crashed.
                    answer.Add(transaction);
                };
            };
            return answer;
        }
        #endregion

        #region Sampling Control
        ////////////////////////////////////////////////////////////////
        //    EXAMPLE TRANSACTION "config" EXTRACTED FROM PLATFORM AppLoad RESPONSE JSON
        // {"defaultTimeout":3600000,
        //  "interval":10,
        //  "enabled":true,
        //  "transactions":{"Buy Critter Feed":{"timeout":60000,"slowness":3600000,"value":1299},
        //                  "Sing Critter Song":{"timeout":90000,"slowness":3600000,"value":1500},
        //                  "Write Critter Poem":{"timeout":60000,"slowness":3600000,"value":2000}}}
        // See example in AppLoad.cs for context.
        ////////////////////////////////////////////////////////////////
        internal static void SettingsChange() {
            try {
                if (Crittercism.Settings != null) {
                    JObject config = Crittercism.Settings["txnConfig"] as JObject;
                    if (config["enabled"] != null) {
                        bool enabled = (bool)((JValue)(config["enabled"])).Value;
                        if (enabled) {
                            int interval = Convert.ToInt32(((JValue)(config["interval"])).Value);
                            int defaultTimeout = Convert.ToInt32(((JValue)(config["defaultTimeout"])).Value);
                            JObject thresholds = config["transactions"] as JObject;
                            Enable(interval,defaultTimeout,thresholds);
                        } else {
                            Disable();
                        }
                    }
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        private static void Enable(int interval,int defaultTimeout,JObject thresholds) {
            ////////////////////////////////////////////////////////////////
            // Input:
            //     interval == milliseconds (millisecond == 10^-3 seconds)
            //     defaultTimeout == milliseconds (millisecond == 10^-3 seconds)
            //     thresholds == as received from platform AppLoad response
            //                   (Dictionary mapping string names to times in milliseconds)
            ////////////////////////////////////////////////////////////////
            lock (lockObject) {
                enabled = true;
                TransactionReporter.interval = interval;
                TransactionReporter.defaultTimeout = defaultTimeout;
                TransactionReporter.thresholds = thresholds;
            }
        }
        private static void Disable() {
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
                if (thresholds != null) {
                    JObject threshold = thresholds[name] as JObject;
                    if (threshold != null) {
                        // thresholdTimeout in milliseconds
                        JValue timeoutValue = threshold["timeout"] as JValue;
                        if (timeoutValue != null) {
                            double thresholdTimeout = Convert.ToDouble(timeoutValue.Value);
                            if ((thresholdTimeout > 0.0) && (answer > thresholdTimeout)) {
                                answer = (int)thresholdTimeout;
                            }
                        }
                    } else {
                        ////////////////////////////////////////////////////////////////
                        // Don't go over global "defaultTimeout" milliseconds
                        ////////////////////////////////////////////////////////////////
                        answer = Math.Min(answer,defaultTimeout);
                    }
                }
            }
            return answer;
        }
        internal static int DefaultValue(string name) {
            // Default value of transaction name (kind of) according to Wire+Protocol doc
            // https://crittercism.atlassian.net/wiki/display/DEV/Wire+Protocol
            // details regarding txnConfig thresholds dictionaries specifying "value"s.

            int answer = Transaction.NULL_VALUE;
            lock (lockObject) {
                if (thresholds != null) {
                    JObject threshold = thresholds[name] as JObject;
                    if (threshold != null) {
                        // thresholdValue in U.S. pennies
                        JValue valueValue = threshold["value"] as JValue;
                        if (valueValue != null) {
                            int thresholdValue = Convert.ToInt32(valueValue.Value);
                            if (thresholdValue >= 0) {
                                answer = thresholdValue;
                            }
                        }
                    }
                }
            }
            return answer;
        }
        #endregion
    }
}
