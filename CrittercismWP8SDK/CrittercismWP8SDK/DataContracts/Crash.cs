// file:	DataContracts\Crash.cs
// summary:	Implements the crash class (Unhandled Exception)
namespace CrittercismSDK.DataContracts
{
    using Microsoft.Phone.Net.NetworkInformation;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using Windows.Devices.Sensors;
    using Windows.Graphics.Display;

    /// <summary>
    /// Crash (Unhandled Exception).
    /// </summary>
    [DataContract]
    public class Crash : MessageReport
    {
        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        [DataMember]
        public string app_id { get; internal set; }

        /// <summary>
        /// Gets or sets the state of the application.
        /// </summary>
        /// <value> The application state. </value>
        [DataMember]
        public Dictionary<string, object> app_state { get; internal set; }

        /// <summary>
        /// Gets or sets the breadcrumbs.
        /// </summary>
        /// <value> The breadcrumbs. </value>
        [DataMember]
        public Breadcrumbs breadcrumbs { get; internal set; }

        [DataMember]
        public Dictionary<string, string> metadata { get; internal set; }

        /// <summary>
        /// Gets or sets the crash
        /// </summary>
        [DataMember]
        public ExceptionObject crash { get; internal set; }

        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value> The platform. </value>
        [DataMember]
        public Platform platform { get; internal set; }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Crash()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appId">Identifier for the application.</param>
        /// <param name="appVersion">The app version.</param>
        /// <param name="currentBreadcrumbs">The current breadcrumbs.</param>
        /// <param name="exception">The exception.</param>
        public Crash(string appId, string appVersion, Dictionary<string,string> currentMetadata, Breadcrumbs currentBreadcrumbs, ExceptionObject exception)
        {
            app_id = appId;
            // Getting lots of stuff here. Some things like "DeviceId" require manifest-level authorization so skipping
            // those for now, see http://msdn.microsoft.com/en-us/library/ff769509%28v=vs.92%29.aspx#BKMK_Capabilities

            // FIXME jbley pull a good chunk of this stuff up to a base class
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
            metadata = currentMetadata;
            breadcrumbs = currentBreadcrumbs;
            crash = exception;
            platform = new Platform();
        }
    }
}
