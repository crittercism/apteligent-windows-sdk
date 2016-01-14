using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
#if WINDOWS_PHONE
using Microsoft.Phone.Info;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Microsoft.Phone.Net.NetworkInformation;
#endif

namespace CrittercismSDK {
    /// <summary>
    /// Message report.
    /// </summary>
    public abstract class MessageReport {

        #region Properties
        /// <summary>
        /// Gets or sets the file name.  E.g. Crash_guid
        /// </summary>
        /// <value> The name. </value>
        internal string Name { get; set; }

        /// <summary>
        /// Gets or sets the date of the file creation.
        /// </summary>
        /// <value> The date of the creation. </value>
        internal DateTimeOffset CreationTime { get; set; }

        private bool Saved { get; set; }
        #endregion

        #region Init

        private static string MessagesPath;

        internal static void Init() {
            MessagesPath = Path.Combine(StorageHelper.CrittercismPath(),"Messages");
            if (!StorageHelper.FolderExists(MessagesPath)) {
                StorageHelper.CreateFolder(MessagesPath);
            }
        }

        #endregion

        #region Instance Methods
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MessageReport() {
            Saved = false;
        }

        internal static Dictionary<string,object> ComputeLegacyAppState() {
            // Used by CrashReport and HandledException report.
            // Getting lots of stuff here. Some things like "DeviceId" require manifest-level authorization so skipping
            // those for now, see http://msdn.microsoft.com/en-us/library/ff769509%28v=vs.92%29.aspx#BKMK_Capabilities

            return new Dictionary<string,object> {
                { "app_version", Crittercism.AppVersion },
                // RemainingChargePercent returns an integer in [0,100]
#if WINDOWS_PHONE
                { "battery_level", Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent / 100.0 },
                { "carrier", DeviceNetworkInformation.CellularMobileOperator },
                { "device_total_ram_bytes", DeviceExtendedProperties.GetValue("DeviceTotalMemory") },
                { "memory_usage", DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage") },
                { "memory_usage_peak", DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage") },
                { "on_cellular_data", DeviceNetworkInformation.IsCellularDataEnabled },
                { "on_wifi", DeviceNetworkInformation.IsWiFiEnabled },
                { "orientation", DisplayProperties.NativeOrientation.ToString() },
#endif
                { "disk_space_free", StorageHelper.AvailableFreeSpace() },
                // skipping "name" for device name as it requires manifest approval
                { "locale", CultureInfo.CurrentCulture.Name},
                // all counters below in bytes
                { "reported_at", TimeUtils.GMTDateString(DateTime.UtcNow) }
            };
        }
        internal static Dictionary<string,object> ComputeAppState() {
            // Used by AppLoad and UserflowReport
            // NOTE: ComputeAppState() isn't identical to ComputeLegacyAppState() .
            Dictionary<string,object> answer = new Dictionary<string,object>();
            answer["appVersion"] = Crittercism.AppVersion;
            answer["appVersion"] = Crittercism.AppVersion;
            answer["osName"] = Crittercism.OSName;
            answer["crPlatform"] = "windows";
            answer["osVersion"] = Crittercism.OSVersion;
            answer["appID"] = Crittercism.AppID;
            answer["locale"] = CultureInfo.CurrentCulture.Name;
            answer["deviceModel"] = Crittercism.DeviceModel;
            answer["appVersion"] = Crittercism.AppVersion;
            answer["deviceID"] = Crittercism.DeviceId;
#if WINDOWS_PHONE
            answer["carrier"] = DeviceNetworkInformation.CellularMobileOperator;
#else
            answer["carrier"] = "UNKNOWN";
#endif
            answer["crVersion"] = "2.2.4";
            return answer;
        }

        /// <summary>
        /// Saves the message to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool Save() {
            // On-disk serialization in JSON alone isn't C# type-preserving.
            // So, this.GetType().Name+"_" is prefixed to file Name .
            bool answer = false;
            try {
                lock (this) {
                    if (!Saved) {
                        Name = this.GetType().Name + "_" + Guid.NewGuid().ToString() + ".js";
                        string path = Path.Combine(MessagesPath,Name);
                        StorageHelper.Save(this,path);
                        Saved = true;
                        answer = true;
                    }
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            };
            return answer;
        }

        /// <summary>
        /// Deletes the message from disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool Delete() {
            bool answer = false;
            try {
                lock (this) {
                    if (Saved) {
                        string path = Path.Combine(MessagesPath,this.Name);
                        if (StorageHelper.FileExists(path)) {
                            StorageHelper.DeleteFile(path);
                            Saved = false;
                        }
                    }
                    answer = true;
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            };
            return answer;
        }
        internal virtual string ContentType() {
            // Most MessageReport's are POST'd via JSON .
            // The exceptional legacy MetadataReport overrides this method. 
            return "application/json; charset=utf-8";
        }
        internal HttpWebRequest WebRequest() {
            HttpWebRequest answer = Crittercism.appLocator.GetWebRequest(GetType());
            if (answer != null) {
                answer.Method = "POST";
                answer.ContentType = ContentType();
            };
            return answer;
        }
        internal virtual string PostBody() {
            // Most MessageReport's are POST'd via JSON .
            // The exceptional legacy MetadataReport overrides this method.
            // Appload overrides this method too.
            return JsonConvert.SerializeObject(this);
        }
        #endregion

        #region Static Methods
        internal static List<MessageReport> LoadMessages() {
            List<MessageReport> answer = new List<MessageReport>();
            if (StorageHelper.FolderExists(MessagesPath)) {
                string[] names = StorageHelper.GetFileNames(MessagesPath);
                foreach (string name in names) {
                    MessageReport messageReport = LoadMessage(name);
                    if (messageReport != null) {
                        answer.Add(messageReport);
                    }
                }
                answer.Sort((m1,m2) => m1.CreationTime.CompareTo(m2.CreationTime));
            }
            return answer;
        }
        internal static MessageReport LoadMessage(string name) {
            // name is wrt MessagesPath "Crittercism\Messages", e.g "Crash_<guid>"
            // path is Crittercism\Messages\name
            MessageReport messageReport = null;
            try {
                string path = Path.Combine(MessagesPath,name);
                string[] nameSplit = name.Split('_');
                switch (nameSplit[0]) {
                    case "AppLoad":
                        messageReport = (AppLoad)StorageHelper.Load(path,typeof(AppLoad));
                        break;
                    case "APMReport":
                        messageReport = (APMReport)StorageHelper.Load(path,typeof(APMReport));
                        break;
                    case "HandledException":
                        messageReport = (HandledException)StorageHelper.Load(path,typeof(HandledException));
                        break;
                    case "CrashReport":
                        messageReport = (CrashReport)StorageHelper.Load(path,typeof(CrashReport));
                        break;
                    case "MetadataReport":
                        messageReport = (MetadataReport)StorageHelper.Load(path,typeof(MetadataReport));
                        break;
                    default:
                        // Skip this file.
                        break;
                }
                if (messageReport == null) {
                    // Possibly file is still being written.  Skip file for
                    // now by returning null .
                } else {
                    messageReport.Name = name;
                    messageReport.CreationTime = StorageHelper.GetCreationTime(path);
                    messageReport.Saved = true;
                }
            } catch (Exception ie) {
                Crittercism.LogInternalException(ie);
            }
            return messageReport;
        }

        #endregion

        #region Response Processing
        internal virtual void DidReceiveResponse(string responseText) {
            // Most MessageReport's ignore the responseText .
            // However, AppLoad which does care, overrides this method.
        }
        #endregion
    }
}
