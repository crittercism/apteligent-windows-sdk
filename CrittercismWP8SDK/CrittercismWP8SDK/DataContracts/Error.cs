// file:	DataContracts\Error.cs
// summary:	Implements the error class
namespace CrittercismSDK.DataContracts
{
    using Microsoft.Phone.Net.NetworkInformation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Windows.Devices.Sensors;
    using Windows.Graphics.Display;

    /// <summary>
    /// Error.
    /// </summary>
    [DataContract]
    public class Error : MessageReport
    {
        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        [DataMember]
        public string app_id { get; internal set; }

        /// <summary>
        /// Gets or sets the application state.
        /// </summary>
        [DataMember]
        public Dictionary<string, object> app_state { get; internal set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value> The error. </value>
        [DataMember]
        public ExceptionObject error { get; internal set; }

        /// <summary>
        /// Gets or sets the platform
        /// </summary>
        [DataMember]
        public Platform platform { get; internal set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Error()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">     Identifier for the application. </param>
        /// <param name="exception"> The exception. </param>
        public Error(string appId, string appVersion, ExceptionObject exception)
        {
            app_id = appId;
            // Initialize app state dictionary with base battery level and app version keys
            app_state = new Dictionary<string, object> {
                { "app_version", String.IsNullOrEmpty(appVersion) ? "Unspecified" : appVersion },
                // RemainingChargePercent returns an integer in [0,100]
                { "battery_level", Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent / 100.0 },
                { "carrier", DeviceNetworkInformation.CellularMobileOperator },
                { "disk_space_free", System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().AvailableFreeSpace },
                { "device_total_ram_bytes", Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceTotalMemory") },
                // skipping "name" for device name as it requires manifest approval
                // all counters below in bytes
                { "memory_usage", Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage") },
                { "memory_usage_peak", Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage") },
                { "on_cellular_data", DeviceNetworkInformation.IsCellularDataEnabled },
                { "on_wifi", DeviceNetworkInformation.IsWiFiEnabled },
                { "orientation", DisplayProperties.NativeOrientation.ToString() },
                { "reported_at", DateTime.Now }
            };

            error = exception;
            platform = new Platform();
        }
    }
}
