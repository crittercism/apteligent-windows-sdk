// UnifiedAppLoadMessage.cs
// David R. Albrecht for Crittercism, Inc.
//
// Implementes a .NET DataContract for the new single app load format we use across all libraries.
// Eventually we'll retire the old data format; I've started moving it aside and calling it
// "legacy". Apologies for the code duplication, eventually we'll only have the unified
// messages/handlers.
//
// There is so much wrong with this code I don't even know where to begin...we should be using
// immutable data objects, we should use an OS-provided persistence framework rather than rolling
// our own with files, etc...fix this as time allows

namespace CrittercismSDK.DataContracts.Unified {

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Runtime.Serialization;

    [DataContract]
    internal class AppLoadInner {
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

        public AppLoadInner(string _appID) {
            this.appID = _appID;
            this.deviceID = StorageHelper.GetOrCreateDeviceId();
        }
    }
    
    [DataContract]
    internal class AppLoad : UnifiedMessageReport {
        [DataMember]
        public AppLoadInner appLoads { get; set; }
        [DataMember]
        public int count = 1;
        [DataMember]
        public bool current = true;

        public AppLoad(string _appID) {
            this.appLoads = new AppLoadInner(_appID);
        }
        public AppLoad() { }
    }
}
