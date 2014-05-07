// UnifiedMessageReport.cs
// David R. Albrecht for Crittercism, Inc.
//
// Implements behavior common to all Crittercism new (unified) API message.
// We provide a common superclass for all messages which overrides/changes some
// behaviors which change from the legacy to unified API. Eventually, this class
// should be combined into MessageReport, at which time MessageReport can be pruned
// of unused functionality, and one of the two can get deleted.

using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using Windows.Graphics.Display;

namespace CrittercismSDK.DataContracts.Unified {
    internal class UnifiedMessageReport : CrittercismSDK.DataContracts.MessageReport {
        /// <summary>
        /// Gets Windows Phone-specific App Load Data
        /// </summary>
        /// Yes, there's a lot of duplication here from MessageReport, but we'll fix it after we
        /// move fully to the new, unified handlers.
        /// <returns></returns>
        public static Dictionary<string, object> GetPlatformSpecificData() {
            return new Dictionary<string, object> {
                // RemainingChargePercent returns an integer in [0,100]
                { "battery_level", Windows.Phone.Devices.Power.Battery.GetDefault().
                    RemainingChargePercent / 100.0 },
                { "disk_space_free", System.IO.IsolatedStorage.IsolatedStorageFile.
                    GetUserStoreForApplication().AvailableFreeSpace },
                { "device_total_ram_bytes", Microsoft.Phone.Info.DeviceExtendedProperties.
                    GetValue("DeviceTotalMemory") },
                // skipping "name" for device name as it requires manifest approval
                // all counters below in bytes
                { "memory_usage", Microsoft.Phone.Info.DeviceExtendedProperties.
                    GetValue("ApplicationCurrentMemoryUsage") },
                { "memory_usage_peak", Microsoft.Phone.Info.DeviceExtendedProperties.
                    GetValue("ApplicationPeakMemoryUsage") },
                { "on_cellular_data", DeviceNetworkInformation.IsCellularDataEnabled },
                { "on_wifi", DeviceNetworkInformation.IsWiFiEnabled },
                { "orientation", DisplayProperties.NativeOrientation.ToString() },
                { "reported_at", DateTimeString(DateTime.Now) }
            };
        }
    }
}
