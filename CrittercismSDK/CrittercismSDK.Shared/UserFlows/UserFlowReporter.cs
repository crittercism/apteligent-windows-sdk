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
    internal class UserFlowReporter {
        #region Constants
        internal const int MAX_USERFLOW_COUNT = 50;
        #endregion

        #region Properties
        private static Object lockObject = new object();
        private static SynchronizedQueue<UserFlow> UserFlowsQueue { get; set; }
        internal static bool enabled = true;
        internal static volatile bool isForegrounded = true;
        // Batch additional network requests for 20 seconds before sending UserFlowReport .
        private const int MSEC_PER_SEC = 1000;
        private const int ONE_HOUR = 3600 * MSEC_PER_SEC; // milliseconds
        private static int interval = 20 * MSEC_PER_SEC; // milliseconds
        private static int defaultTimeout = ONE_HOUR; // milliseconds
        // private const int ONE_MINUTE = 60 * MSEC_PER_SEC; // milliseconds
        // private static int defaultTimeout = ONE_MINUTE; // milliseconds
        private static JObject thresholds = null;

        internal static int Interval() {
            // UserFlow batch reporting interval in milliseconds
            return interval;
        }

        internal static int DefaultTimeout() {
            // UserFlow default timeout in milliseconds
            return defaultTimeout;
        }
        #endregion

        #region Multithreading Remarks
        ////////////////////////////////////////////////////////////////
        // MULTITHREADING UserFlow AND UserFlowReporter REMARKS
        // * lock "Lock Ordering"
        //    The deadlock prevention technique of establishing a global ordering
        //    on resources which may be locked is known as "Lock Ordering"
        //    Lock ordering is a simple yet effective deadlock prevention mechanism.
        //    http://tutorials.jenkov.com/java-concurrency/deadlock-prevention.html#ordering
        // UserFlow code sometimes needs to lock both a userFlow and
        // the reporter simultaneously.  Our lock deadlock prevention
        // policy is to always obtain lock on userFlow first and lock
        // on reporter second whenever this is necessary.  Effectively:
        //    lock (userFlow) {
        //      lock (UserFlowReporter.lockObject) {
        //        ...
        //      }
        //    }
        // though possibly spread out on the stack trace, not necessarily
        // all in one function or class.
        // * Changing properties of userFlows is done synchronously.
        ////////////////////////////////////////////////////////////////
        #endregion

        #region Life Cycle
        internal static void Init() {
            lock (lockObject) {
                // TODO: Rigorously we should check if app's window is visible just now
                isForegrounded = true;
                // Crittercism.Init calling UserFlowReporter.Init should effectively make
                // lock lockObject here pointless, but no real harm doing so.
                SettingsChange();
                // Initialize userFlowsDictionary and UserFlowsQueue
                userFlowsDictionary = new Dictionary<string,UserFlow>();
                UserFlowsQueue = new SynchronizedQueue<UserFlow>(new Queue<UserFlow>());
            }
        }
        internal static void Shutdown() {
            lock (lockObject) {
                // Crittercism.Shutdown calls UserFlowReporter.Shutdown
                Background();
            }
        }
        #endregion

        #region Persistence
        internal static void Save(UserFlow userFlow) {
            // Persist userFlow to correct directory
            lock (lockObject) {
                switch (userFlow.State()) {
                    case UserFlowState.CANCELLED:
                        CancelUserFlow(userFlow);
                        break;
                    case UserFlowState.CREATED:
                        // Make visible via persistence API methods.
                        AddUserFlow(userFlow);
                        break;
                    case UserFlowState.BEGUN:
                        // Nothing extra to do.
                        break;
                    case UserFlowState.CRASHED:
                        CancelUserFlow(userFlow);
                        break;
                    default:
                        // Final state
                        CancelUserFlow(userFlow);
                        Enqueue(userFlow);
                        break;
                }
            }
        }
        private static void AddUserFlow(UserFlow userFlow) {
            lock (lockObject) {
                userFlowsDictionary[userFlow.Name()] = userFlow;
            }
        }
        private static void CancelUserFlow(UserFlow userFlow) {
            lock (lockObject) {
                userFlowsDictionary.Remove(userFlow.Name());
            }
        }
        #endregion

        #region UserFlow Dictionary
        private static Dictionary<string,UserFlow> userFlowsDictionary;
        internal static int UserFlowCount() {
            int answer = 0;
            lock (lockObject) {
                answer = userFlowsDictionary.Count;
            }
            return answer;
        }
        internal static UserFlow[] AllUserFlows() {
            UserFlow[] answer = null;
            lock (lockObject) {
                List<UserFlow> list = new List<UserFlow>();
                foreach (UserFlow userFlow in userFlowsDictionary.Values) {
                    list.Add(userFlow);
                }
                answer = list.ToArray();
            }
            return answer;
        }
        internal static UserFlow UserFlowForName(string name) {
            UserFlow answer = null;
            lock (lockObject) {
                if (userFlowsDictionary.ContainsKey(name)) {
                    answer = userFlowsDictionary[name];
                }
            }
            return answer;
        }
        #endregion

        #region Timing
        // Different .NET frameworks get different timer's
#if NETFX_CORE || WINDOWS_PHONE
        private static ThreadPoolTimer timer = null;
        private static void OnTimerElapsed(ThreadPoolTimer sender) {
            lock (lockObject) {
                SendUserFlowReport();
                timer = null;
            }
        }
#else
        private static Timer timer=null;
        private static void OnTimerElapsed(Object source, ElapsedEventArgs e) {
            lock (lockObject) {
                SendUserFlowReport();
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

        #region Background / Foreground
        internal static void Background() {
            lock (lockObject) {
                isForegrounded = false;
                long backgroundTime = DateTime.UtcNow.Ticks;
                foreach (UserFlow userFlow in userFlowsDictionary.Values) {
                    userFlow.Background(backgroundTime);
                };
                RemoveTimer();
            }
        }
        internal static void Foreground() {
            lock (lockObject) {
                isForegrounded = true;
                SendUserFlowReport();
                long foregroundTime = DateTime.UtcNow.Ticks;
                foreach (UserFlow userFlow in userFlowsDictionary.Values) {
                    userFlow.Foreground(foregroundTime);
                };
            }
        }
        #endregion

        #region Reporting
        internal static void Enqueue(UserFlow userFlow) {
            lock (lockObject) {
                if (enabled) {
                    while (UserFlowsQueue.Count >= MAX_USERFLOW_COUNT) {
                        UserFlowsQueue.Dequeue();
                    };
                    UserFlowsQueue.Enqueue(userFlow);
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
        private static long BeginTime(List<UserFlow> userFlows) {
            // Earliest BeginTime amongst these userFlows
            long answer = long.MaxValue;
            foreach (UserFlow userFlow in userFlows) {
                answer = Math.Min(answer,userFlow.BeginTime());
            }
            return answer;
        }
        private static long EndTime(List<UserFlow> userFlows) {
            // Latest EndTime amongst these userFlows
            long answer = long.MinValue;
            foreach (UserFlow userFlow in userFlows) {
                answer = Math.Max(answer,userFlow.EndTime());
            }
            return answer;
        }

        private static void SendUserFlowReport() {
            if (UserFlowsQueue.Count > 0) {
                List<UserFlow> userFlows = UserFlowsQueue.ToList();
                UserFlowsQueue.Clear();
                long beginTime = BeginTime(userFlows);
                long endTime = EndTime(userFlows);
                Dictionary<string,object> appState = MessageReport.ComputeAppState();
                List<UserBreadcrumb> breadcrumbs = Breadcrumbs.ExtractUserBreadcrumbs(beginTime,endTime);
                List<Breadcrumb> systemBreadcrumbs = Breadcrumbs.SystemBreadcrumbs().RecentBreadcrumbs(beginTime,endTime);
                List<Endpoint> endpoints = Breadcrumbs.ExtractEndpoints(beginTime,endTime);
                UserFlowReport userFlowReport = new UserFlowReport(
                    appState,
                    userFlows,
                    breadcrumbs,
                    systemBreadcrumbs,
                    endpoints);
                Crittercism.AddMessageToQueue(userFlowReport);
            }
        }
        internal static List<UserFlow> CrashUserFlows() {
            // Remove BEGUN UserFlow's to CRASHED state.
            // The code takes care to avoid deadlocks by not locking
            // the reporter and then locking a UserFlow (wrong order).
            UserFlow[] allUserFlows = AllUserFlows();
            // Compute crashed UserFlow's .
            List<UserFlow> answer = new List<UserFlow>();
            foreach (UserFlow userFlow in allUserFlows) {
                // Request userFlow to Crash.
                userFlow.Crash();
                if (userFlow.State() == UserFlowState.CRASHED) {
                    // The userFlow crashed.
                    answer.Add(userFlow);
                };
            };
            return answer;
        }
        #endregion

        #region Sampling Control
        ////////////////////////////////////////////////////////////////
        //    EXAMPLE USERFLOW "config" EXTRACTED FROM PLATFORM AppLoad RESPONSE JSON
        // {"defaultTimeout":3600000,
        //  "interval":10,
        //  "enabled":true,
        //  "transactions":{"Buy Critter Feed":{"timeout":60000,"slowness":3600000,"value":1299},
        //                  "Sing Critter Song":{"timeout":90000,"slowness":3600000,"value":1500},
        //                  "Write Critter Poem":{"timeout":60000,"slowness":3600000,"value":2000}}}
        // See example in AppLoad.cs for context.
        // * OMG.  Platform is sending "defaultTimeout" in milliseconds
        // and "interval" in seconds.
        ////////////////////////////////////////////////////////////////
        internal static void SettingsChange() {
            try {
                if (Crittercism.Settings != null) {
                    JObject config = Crittercism.Settings["txnConfig"] as JObject;
                    if (config["enabled"] != null) {
                        bool enabled = (bool)((JValue)(config["enabled"])).Value;
                        if (enabled) {
                            // NOTE: Platform sends "interval" in seconds, but method Enable wants
                            // that time converted to milliseconds.
                            int interval = (int)(Convert.ToDouble(((JValue)(config["interval"])).Value) * MSEC_PER_SEC);
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
                if (interval<1000) {
                    Debug.WriteLine("THIS SHOULDN'T HAPPEN");
                }
                UserFlowReporter.interval = interval;
                UserFlowReporter.defaultTimeout = defaultTimeout;
                UserFlowReporter.thresholds = thresholds;
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
            // txnConfig userFlows thresholds dictionaries.
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
            // Default value of userFlow name (kind of) according to Wire+Protocol doc
            // https://crittercism.atlassian.net/wiki/display/DEV/Wire+Protocol
            // details regarding txnConfig thresholds dictionaries specifying "value"s.

            int answer = UserFlow.NULL_VALUE;
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
