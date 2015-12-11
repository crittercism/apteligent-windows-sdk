////////////////////////////////////////////////////////////////
// TODO: UnfifiedAppLoad.cs is some D.A. code that was never
// completely put into production.  Currently dead weight.
// It might be deleted in the future.
////////////////////////////////////////////////////////////////
#if false

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

// David R. Albrecht for Crittercism, Inc.
//
// This library has a lot of room for improvement. A few areas I saw working on this in May 2014:
//   (1) More prevalent use of immutable data objects is good defensive programming against errors

namespace CrittercismSDK {

    [DataContract]
    internal class UnifiedAppLoadInner : System.IEquatable<UnifiedAppLoadInner> {
        [DataMember]
        public string appID;
        [DataMember]
        public string deviceID;
        [DataMember]
        public string crPlatform = "windows";
        [DataMember]
        public string crVersion=Crittercism.Version;
        [DataMember]
        public string deviceModel="PC";
        [DataMember]
        public string osName=Crittercism.OSName;
        [DataMember]
        public string osVersion=Crittercism.OSVersion;
        [DataMember]
        public string carrier=Crittercism.Carrier;
        [DataMember]
        public string appVersion=Crittercism.AppVersion;
        [DataMember]
        public string locale = CultureInfo.CurrentCulture.Name;
        [DataMember]
        public Dictionary<string, object> platformSpecificData =
            UnifiedMessageReport.GetPlatformSpecificData();

        public UnifiedAppLoadInner(string _appID) {
            this.appID=_appID;
            this.deviceID=Crittercism.DeviceId;
            this.deviceModel=Crittercism.DeviceModel;
        }

        private static bool platformSpecificDataEqual(Dictionary<string, object> x, Dictionary<string, object> y) {
            return x.Keys.Count == y.Keys.Count && 
                x.Keys.All(k => y.ContainsKey(k) && object.Equals(x[k], y[k]));
        }

        public bool Equals(UnifiedAppLoadInner other) {
            // TODO: Convert this class into some kind of struct or value type -- this nonsense
            // shouldn't be necessary
            return (appID == other.appID && deviceID == other.deviceID &&
                crPlatform == other.crPlatform && crVersion == other.crVersion &&
                deviceModel == other.deviceModel && osName == other.osName &&
                osVersion == other.osVersion && carrier == other.carrier &&
                appVersion == other.appVersion && locale == other.locale &&
                platformSpecificDataEqual(platformSpecificData, other.platformSpecificData));
        }
    }
    
    [DataContract]
    internal class UnifiedAppLoad : UnifiedMessageReport, IEquatable<UnifiedAppLoad> {
        [DataMember]
        public UnifiedAppLoadInner appLoads { get; set; }
        [DataMember]
        public int count = 1;
        [DataMember]
        public bool current = true;

        public UnifiedAppLoad(string _appID) {
            this.appLoads = new UnifiedAppLoadInner(_appID);
        }
        public UnifiedAppLoad() { }

        public bool Equals(UnifiedAppLoad other) {
            return appLoads.Equals(other.appLoads) &&
                this.count == other.count &&
                this.current == other.current;
        }
    }
}

#endif // dead weight
