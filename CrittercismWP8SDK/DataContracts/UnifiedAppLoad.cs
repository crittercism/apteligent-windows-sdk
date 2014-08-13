// UnifiedAppLoad.cs
// David R. Albrecht for Crittercism, Inc.
//
// This library has a lot of room for improvement. A few areas I saw working on this in May 2014:
//   (1) More prevalent use of immutable data objects is good defensive programming against errors
//   (2) Currently we persist objects to "disk" manually -- is there an OS-provided storage system
//       (maybe IsolatedStorage) or a .NET-provided object persistence system we can use, rather
//       than writing raw files?

namespace CrittercismSDK.DataContracts {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Runtime.Serialization;

    [DataContract]
    internal class UnifiedAppLoadInner : System.IEquatable<UnifiedAppLoadInner> {
        [DataMember]
        public string appID;
        [DataMember]
        public Guid deviceID;
        [DataMember]
        public string crPlatform = "windows";
        [DataMember]
        public string crVersion = System.Reflection.Assembly.GetExecutingAssembly().
            GetName().Version.ToString();
        [DataMember]
        public string deviceModel = Microsoft.Phone.Info.DeviceStatus.DeviceName;
        [DataMember]
        public string osName = "wp";
        [DataMember]
        public string osVersion = Environment.OSVersion.Version.ToString();
        [DataMember]
        public string carrier = Microsoft.Phone.Net.NetworkInformation.
            DeviceNetworkInformation.CellularMobileOperator;
        [DataMember]
        // Note that GetExecutingAssembly wouldn't work because we (Crittercism) *are* the executing assembly
        public string appVersion = System.Windows.Application.Current.GetType().
            Assembly.GetName().Version.ToString();
        [DataMember]
        public string locale = System.Globalization.CultureInfo.CurrentCulture.Name;
        [DataMember]
        public Dictionary<string, object> platformSpecificData =
            UnifiedMessageReport.GetPlatformSpecificData();

        public UnifiedAppLoadInner(string _appID) {
            this.appID = _appID;
            this.deviceID = StorageHelper.GetOrCreateDeviceId();
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
