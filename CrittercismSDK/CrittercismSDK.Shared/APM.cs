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
        // TODO: SynchronizedQueue has its virtues, but we may want synchronization
        // at the higher APM level instead.
        private static Object lockObject=new Object();

        // Different .NET frameworks get different timer's
#if NETFX_CORE || WINDOWS_PHONE
        ThreadPoolTimer timer=null;
        private static void OnTimerElapsed(ThreadPoolTimer timer) {
            SendNetworkEndpoints();
        }
#else
        Timer timer=null;
        private static void OnTimerElapsed(Object source, ElapsedEventArgs e) {
            SendNetworkEndpoints();
        }
#endif // NETFX_CORE

        // CRFilter's
        private static List<CRFilter> Filters;

        // Collect APMEndpoint's
        const int MAX_NETWORK_STATS=100;
        private static SynchronizedQueue<APMEndpoint> EndpointsQueue { get; set; }

        internal static void Enqueue(APMEndpoint endpoint) {
            lock (lockObject) {
                while (EndpointsQueue.Count>=MAX_NETWORK_STATS) {
                    EndpointsQueue.Dequeue();
                };
                EndpointsQueue.Enqueue(endpoint);
                // TODO: We'd like sendNetworkEndpoints triggered by an elapsed Timer
                SendNetworkEndpoints();
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

        private static void SendNetworkEndpoints() {
            // Sending in batches of 3 endpoints should let us P.O.C. before
            // we are finished implementing Timer's 
            if (EndpointsQueue.Count>=3) {
                APMEndpoint[] endpoints=EndpointsQueue.ToArray();
                EndpointsQueue.Clear();
                APMReport apmReport=new APMReport(AppIdentifiersArray(),DeviceStateArray(),endpoints);
                Crittercism.AddMessageToQueue(apmReport);
            }
        }

        internal static void Init() {
            lock (lockObject) {
                // Crittercism.Init calling APM.Init should effectively make
                // lock lockObject here pointless, but no real harm doing so.
                Filters=new List<CRFilter>();
                EndpointsQueue=new SynchronizedQueue<APMEndpoint>(new Queue<APMEndpoint>());
            }
        }

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
    }
}
