using System;
using System.Runtime.Serialization;
#if WINDOWS_PHONE
using Microsoft.Phone.Info;
#endif

// David R. Albrecht for Crittercism, Inc.

namespace CrittercismSDK {

    /// <summary>
    /// Represents the platform on which the application (app) is running.
    /// </summary>
    [DataContract]
    internal class Platform {
        /// <summary>
        /// Identifies this library to Crittercism
        /// </summary>
        [DataMember]
        public readonly string client = "wp8v3.0.1"; // FIXME JBLEY check before shipping, possibly determine dynamically

        /// <summary>
        /// A GUID identifying this device
        /// </summary>
        [DataMember]
        public readonly string device_id;

        /// <summary>
        /// What kind of device is this? e.g. "Nokia Lumia 820"
        /// </summary>
        [DataMember]
        public readonly string device_model = Crittercism.DeviceModel;

        [DataMember]
        public readonly string os_name = Crittercism.OSName;

        [DataMember]
        public readonly string os_version = Crittercism.OSVersion;

        public Platform() {
            this.device_id = Crittercism.DeviceId;
        }
    }
}
