using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.ApplicationModel;
#elif WINDOWS_PHONE
using Microsoft.Phone.Info;
#endif

namespace CrittercismSDK
{
    [DataContract]
    internal class AppLoad : MessageReport {
        #region Properties
        [DataMember]
        public Dictionary<string,object> appLoads;
        [DataMember]
        public int count = 1;
        [DataMember]
        public bool current = true;
        #endregion

        #region Constructor
        public AppLoad() {
            appLoads = MessageReport.ComputeAppState();
        }
        #endregion

        #region JSON
        internal override string PostBody() {
            // AppLoad PostBody is an exceptional override.
            // Instead of inventing a whole new class and JSON
            // serialization code just to get an Appload put into a JSON
            // array, we instead create another specialized override
            // of the MessageReport.cs "internal virtual string PostBody()"
            return "[" + JsonConvert.SerializeObject(this) + "]";
        }

        ////////////////////////////////////////////////////////////////
        //    EXAMPLE AppLoad RESPONSE JSON FROM PLATFORM
        // {"txnConfig":{"defaultTimeout":3600000,
        //               "interval":10,
        //               "enabled":true,
        //               "transactions":{"Buy Critter Feed":{"timeout":60000,"slowness":3600000,"value":1299},
        //                               "Sing Critter Song":{"timeout":90000,"slowness":3600000,"value":1500},
        //                               "Write Critter Poem":{"timeout":60000,"slowness":3600000,"value":2000}}},
        //  "apm":{"net":{"enabled":true,
        //                "persist":false,
        //                "interval":10}},
        //  "needPkg":1,
        //  "internalExceptionReporting":true}
        ////////////////////////////////////////////////////////////////
        internal static void DidReceiveResponse(string json) {
            // Process AppLoad response JSON from platform.
            try {
                Debug.WriteLine(json);
                JObject response = JToken.Parse(json) as JObject;
                if (response != null) {
                    {
                        JObject txnConfig = response["txnConfig"] as JObject;
                        Debug.WriteLine("txnConfig == " + JsonConvert.SerializeObject(txnConfig));
                        // TODO: Process "txnConfig" .
                    }
                    {
                        JObject apm = response["apm"] as JObject;
                        Debug.WriteLine("apm == " + JsonConvert.SerializeObject(apm));
                        // TODO: Process "apm" .
                    }
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        #endregion
    }
}
