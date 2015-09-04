using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
//using System.Timers;

namespace CrittercismSDK
{

    internal class APM
    {
        // TODO: SynchronizedQueue has its virtues, but we may want synchronization
        // at the higher APM level instead.

        // TODO: Different .NET frameworks have different Timer's?  May get complicated.
        //Timer timer=new Timer(10000);

        // Collect APMEndpoint's
        const int MAX_NETWORK_STATS=100;
        private static SynchronizedQueue<APMEndpoint> EndpointsQueue { get; set; }

        internal static void Enqueue(APMEndpoint endpoint) {
            while (EndpointsQueue.Count>=MAX_NETWORK_STATS) {
                EndpointsQueue.Dequeue();
            };
            EndpointsQueue.Enqueue(endpoint);
            // TODO: We'd like sendNetworkEndpoints triggered by an elapsed Timer
            SendNetworkEndpoints();
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

        internal static void SendNetworkEndpoints() {
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
            EndpointsQueue=new SynchronizedQueue<APMEndpoint>(new Queue<APMEndpoint>());
        }
    }
}
