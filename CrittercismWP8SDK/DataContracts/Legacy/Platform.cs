// Platform.cs
// David R. Albrecht for Crittercism, Inc.

namespace CrittercismSDK.DataContracts.Legacy {
    using Microsoft.Phone.Info;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the platform on which the application (app) is running.
    /// </summary>
    [DataContract]
    public class Platform {
        /// <summary>
        /// Identifies this library to Crittercism
        /// </summary>
        [DataMember]
        public readonly string client = "wp8v2.0"; // FIXME JBLEY check before shipping, possibly determine dynamically

        /// <summary>
        /// A GUID identifying this device
        /// </summary>
        [DataMember]
        public readonly string device_id;

        /// <summary>
        /// What kind of device is this? e.g. "Nokia Lumia 820"
        /// </summary>
        [DataMember]
        public readonly string device_model = DeviceStatus.DeviceName;

        [DataMember]
        public readonly string os_name = "wp";

        [DataMember]
        public readonly string os_version = Environment.OSVersion.Version.ToString();

        public Platform() {
            this.device_id = StorageHelper.GetOrCreateDeviceId().ToString();
        }
    }
}
