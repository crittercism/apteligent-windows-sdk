using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        #endregion
    }
}
