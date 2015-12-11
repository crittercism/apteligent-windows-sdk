////////////////////////////////////////////////////////////////
// TODO: UnifiedMessageReport.cs is some D.A. code that was never
// completely put into production.  Currently dead weight.
// It might be deleted in the future.
////////////////////////////////////////////////////////////////
#if false

// UnifiedMessageReport.cs
// David R. Albrecht for Crittercism, Inc.
//
// Implements behavior common to all Crittercism new (unified) API message.
// We provide a common superclass for all messages which overrides/changes some
// behaviors which change from the legacy to unified API. Eventually, this class
// should be combined into MessageReport, at which time MessageReport can be pruned
// of unused functionality, and one of the two can get deleted.

using System;
using System.Collections.Generic;
#if WINDOWS_PHONE
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
using Windows.Graphics.Display;
using Windows.Phone.Devices.Power;
#endif

namespace CrittercismSDK {
    internal class UnifiedMessageReport : MessageReport {
        /// <summary>
        /// Gets Windows Phone-specific App Load Data
        /// </summary>
        /// Yes, there's a lot of duplication here from MessageReport, but we'll fix it after we
        /// move fully to the new, unified handlers.
        /// <returns></returns>
        public static Dictionary<string,object> GetPlatformSpecificData() {
            return new Dictionary<string,object> {
#if WINDOWS_PHONE
                // RemainingChargePercent returns an integer in [0,100]
                { "battery_level", Battery.GetDefault().RemainingChargePercent },
                { "disk_space_free", StorageHelper.AvailableFreeSpace() },
                { "device_total_ram_bytes", DeviceExtendedProperties.GetValue("DeviceTotalMemory") },
                // skipping "name" for device name as it requires manifest approval
                // all counters below in bytes
                { "memory_usage", DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage") },
                { "memory_usage_peak", DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage") },
                { "on_cellular_data", DeviceNetworkInformation.IsCellularDataEnabled },
                { "on_wifi", DeviceNetworkInformation.IsWiFiEnabled },
                { "orientation", DisplayProperties.NativeOrientation.ToString() }
#endif
                                                 };
        }
    }
}

#endif // dead weight
