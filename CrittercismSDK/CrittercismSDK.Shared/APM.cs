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
    internal class APM
    {
        #region Constants
        // Collect APMEndpoint's
        const int MAX_NETWORK_STATS = 100;
        #endregion

        #region Properties
        private static Object lockObject = new Object();
        // TODO: SynchronizedQueue has its virtues, but we may want synchronization
        // at the higher APM level instead.
        private static SynchronizedQueue<APMEndpoint> EndpointsQueue { get; set; }
        // Batch additional network requests for 10 seconds before sending APMReport .
        internal static int NETWORK_SEND_INTERVAL = 10000;
        // CRFilter's
        private static HashSet<CRFilter> Filters;
        #endregion

        #region Life Cycle
        internal static void Init() {
            lock (lockObject) {
                // Crittercism.Init calling APM.Init should effectively make
                // lock lockObject here pointless, but no real harm doing so.
                Filters = new HashSet<CRFilter>();
                EndpointsQueue = new SynchronizedQueue<APMEndpoint>(new Queue<APMEndpoint>());
            }
        }
        internal static void Shutdown() {
            lock (lockObject) {
                // Crittercism.Shutdown calls APM.Shutdown
                RemoveTimer();
            }
        }
        #endregion

        #region Timing
        // Different .NET frameworks get different timer's
#if NETFX_CORE || WINDOWS_PHONE
        private static ThreadPoolTimer timer=null;
        private static void OnTimerElapsed(ThreadPoolTimer timer) {
            lock (lockObject) {
                SendAPMReport();
                timer=null;
            }
        }
#else
        private static Timer timer=null;
        private static void OnTimerElapsed(Object source, ElapsedEventArgs e) {
            lock (lockObject) {
                SendAPMReport();
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
        internal static void Enqueue(APMEndpoint endpoint) {
            Debug.WriteLine("APM Enqueue");
            lock (lockObject) {
                while (EndpointsQueue.Count>=MAX_NETWORK_STATS) {
                    EndpointsQueue.Dequeue();
                };
                EndpointsQueue.Enqueue(endpoint);
                Debug.WriteLine("APM NETWORK_SEND_INTERVAL == "+ NETWORK_SEND_INTERVAL);
#if NETFX_CORE || WINDOWS_PHONE
                if (timer==null) {
                    // Creates a single-use timer.
                    // https://msdn.microsoft.com/en-US/library/windows/apps/windows.system.threading.threadpooltimer.aspx
                    Debug.WriteLine("APM ThreadPoolTimer.CreateTimer");
                    timer=ThreadPoolTimer.CreateTimer(
                        OnTimerElapsed,
                        TimeSpan.FromMilliseconds(NETWORK_SEND_INTERVAL));
                }
#else
                if (timer==null) {
                    // Generates an event after a set interval
                    // https://msdn.microsoft.com/en-us/library/system.timers.timer(v=vs.110).aspx
                    Debug.WriteLine("APM new Timer");
                    timer = new Timer(NETWORK_SEND_INTERVAL);
                    timer.Elapsed += OnTimerElapsed;
                    // the Timer should raise the Elapsed event only once (false)
                    timer.AutoReset = false;        // fire once
                    timer.Enabled = true;           // Start the timer
                }
#endif // NETFX_CORE
            }
        }

        // App Identifiers Array
        private static Object[] AppIdentifiersArray() {
            // [<app_id>, <app_version>, <device_id>, <cr_version>, <session_id>]
            Object[] answer=new Object[] {
                Crittercism.AppID,
                Crittercism.AppVersion,
                Crittercism.DeviceId,
                Crittercism.Version,
                Crittercism.SessionId
            };
            return answer;
        }

        // Device State Array
        private static Object[] DeviceStateArray() {
            // [
            //   <report_timestamp>,
            //   <carrier>,
            //   <model>,
            //   <cr_platform>,
            //   <os_version>,
            //   <mobile_country_code>,        // optional
            //   <mobile_network_code>         // optional
            //   ]
            Object[] answer=new Object[] {
                DateUtils.ISO8601DateString(DateTime.UtcNow),
                Crittercism.Carrier,
                Crittercism.DeviceModel,
                Crittercism.OSName,
                Crittercism.OSVersion
            };
            return answer;
        }

        private static void SendAPMReport() {
            Debug.WriteLine("SendAPMReport");
            if (EndpointsQueue.Count>0) {
                APMEndpoint[] endpoints=EndpointsQueue.ToArray();
                EndpointsQueue.Clear();
                Debug.WriteLine("SendAPMReport new APMReport");
                APMReport apmReport=new APMReport(AppIdentifiersArray(),DeviceStateArray(),endpoints);
                Crittercism.AddMessageToQueue(apmReport);
            }
        }
        #endregion

        #region Filters
        internal static void AddFilter(CRFilter filter) {
            lock (lockObject) {
                Filters.Add(filter);
            }
        }

        internal static void RemoveFilter(CRFilter filter) {
            lock (lockObject) {
                Filters.Remove(filter);
            }
        }

        internal static bool IsFiltered(string value) {
            bool answer=false;
            lock (lockObject) {
                foreach (CRFilter filter in Filters) {
                    answer=filter.IsMatch(value);
                    if (answer) {
                        break;
                    }
                }
            }
            return answer;
        }
        #endregion
    }
}
