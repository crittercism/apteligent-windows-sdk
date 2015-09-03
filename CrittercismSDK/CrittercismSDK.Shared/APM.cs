using System;
using System.Collections.Generic;
using System.Text;

namespace CrittercismSDK
{

    internal class APM
    {
        // Collect APMEndpoint's
        private static SynchronizedQueue<APMEndpoint> EndpointsQueue { get; set; }

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

        internal void Init() {
            EndpointsQueue=new SynchronizedQueue<APMEndpoint>(new Queue<APMEndpoint>());
        }
    }
}
