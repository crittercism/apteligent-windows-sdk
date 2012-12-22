// Platform.cs
// David R. Albrecht for Crittercism, Inc.

namespace CrittercismSDK.DataContracts {
    using Microsoft.Phone.Info;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the platform on which the application (app) is running.
    /// </summary>
    [DataContract]
    public class Platform {
        private const string DEVICE_ID_KEY = "device_id";

        /// <summary>
        /// Identifies this library to Crittercism
        /// </summary>
        [DataMember]
        public readonly string client = "wp8v1.0";

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
            var storedAppId = GetAppId();
            
            if(storedAppId != null) {
                this.device_id = storedAppId;
            } else {
                this.device_id = CreateStoreNewAppId();
            }
        }

        /// <summary>
        /// Attempts to retrieve the app_id from storage.
        /// </summary>
        /// <returns>String with app_id, null otherwise</returns>
        private string GetAppId() {
            if(System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Contains(DEVICE_ID_KEY)) {
                return System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[DEVICE_ID_KEY] as String;
            } else {
                return null;
            }
        }

        private string CreateStoreNewAppId() {
            var device_id = Guid.NewGuid().ToString();
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[DEVICE_ID_KEY] = device_id;

            return device_id;
        }
    }
}
