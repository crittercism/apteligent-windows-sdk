using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.ApplicationModel;
#elif WINDOWS_PHONE
using Microsoft.Phone.Info;
#endif

namespace CrittercismSDK {
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
        internal override void DidReceiveResponse(string json) {
            // Process AppLoad response JSON from platform.
            try {
                Debug.WriteLine("AppLoad response == " + json);
                // Checking for a sane "response"
                JObject response = null;
                try {
                    response = JToken.Parse(json) as JObject;
                } catch {
                };
                if (Crittercism.CheckSettings(response)) {
                    // There is an AppLoad response JSON we can apply to current session.
                    Crittercism.SaveSettings(json);
                    {
                        JObject config = response["txnConfig"] as JObject;
                        if (config != null) {
                            Debug.WriteLine("txnConfig == " + JsonConvert.SerializeObject(config));
                            TransactionReporter.DidReceiveResponse(config);
                        }
                    }
                    {
                        JObject config = response["apm"] as JObject;
                        if (config != null) {
                            Debug.WriteLine("apm == " + JsonConvert.SerializeObject(config));
                            APM.DidReceiveResponse(config);
                        }
                    }
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
        }
        #endregion
    }
}
