// file:	CrittercismSDK\MessageReport.cs
// summary:	Implements the message report class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Reflection;
    using Microsoft.Phone.Net.NetworkInformation;
    using Windows.Devices.Sensors;
    using Windows.Graphics.Display;

    /// <summary>
    /// Message report.
    /// </summary>
    internal abstract class MessageReport
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        /// <value> The name. </value>
        internal string Name { get; set; }

        /// <summary>
        /// Gets or sets the date of the file creation.
        /// </summary>
        /// <value> The date of the creation. </value>
        internal DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this object is loaded from file.
        /// </summary>
        /// <value> true if this object is loaded, false if not. </value>
        internal bool IsLoaded { get; set; }

        internal static string DateTimeString(DateTime dt)
        {
            return dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK", System.Globalization.CultureInfo.InvariantCulture);
        }

        protected Dictionary<string,object> ComputeAppState(string appVersion)
        {
            // Getting lots of stuff here. Some things like "DeviceId" require manifest-level authorization so skipping
            // those for now, see http://msdn.microsoft.com/en-us/library/ff769509%28v=vs.92%29.aspx#BKMK_Capabilities

            return new Dictionary<string, object> {
                { "app_version", String.IsNullOrEmpty(appVersion) ? "Unspecified" : appVersion },
                // RemainingChargePercent returns an integer in [0,100]
                { "battery_level", Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent / 100.0 },
                { "carrier", DeviceNetworkInformation.CellularMobileOperator },
                { "disk_space_free", System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().AvailableFreeSpace },
                { "device_total_ram_bytes", Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceTotalMemory") },
                // skipping "name" for device name as it requires manifest approval
                { "locale", System.Globalization.CultureInfo.CurrentCulture.Name},
                // all counters below in bytes
                { "memory_usage", Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage") },
                { "memory_usage_peak", Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage") },
                { "on_cellular_data", DeviceNetworkInformation.IsCellularDataEnabled },
                { "on_wifi", DeviceNetworkInformation.IsWiFiEnabled },
                { "orientation", DisplayProperties.NativeOrientation.ToString() },
                { "reported_at", DateTimeString(DateTime.Now) }
            };
        }


        /// <summary>
        /// Saves the message to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool SaveToDisk()
        {
            try
            {
                string folderName = Crittercism.FolderName;
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (!storage.DirectoryExists(folderName))
                {
                    storage.CreateDirectory(folderName);
                }

                Name = this.GetType().Name + "_" + Guid.NewGuid().ToString() + ".txt";
                using (IsolatedStorageFileStream writeFile = new IsolatedStorageFileStream(folderName + "\\" + this.Name, FileMode.CreateNew, FileAccess.Write, storage))
                {
                    // DataContractJsonSerializer serializer = new DataContractJsonSerializer(this.GetType());
                    // serializer.WriteObject(writeFile, this);
                    StreamWriter writer = new StreamWriter(writeFile);
                    writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(this));
                    writer.Flush();
                    writer.Close();
                    this.IsLoaded = true;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes the message from disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool DeleteFromDisk()
        {
            string folderName = Crittercism.FolderName;
            try
            {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.DirectoryExists(folderName))
                {
                    if (storage.FileExists(folderName + "\\" + this.Name))
                    {
                        storage.DeleteFile(folderName + "\\" + this.Name);
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the message from disk.
        /// </summary>
        /// <returns>  true if it succeeds, false if it fails. </returns>
        internal bool LoadFromDisk()
        {
            string folderName = Crittercism.FolderName;
            try
            {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.DirectoryExists(folderName))
                {
                    if (storage.FileExists(folderName + "\\" + this.Name))
                    {
                        using (IsolatedStorageFileStream readFile = storage.OpenFile(folderName + "\\" + this.Name, FileMode.Open, FileAccess.Read))
                        {
                            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(this.GetType());
                            //MessageReport message = (MessageReport)serializer.ReadObject(readFile);
                            StreamReader reader = new StreamReader(readFile);
                            string json = reader.ReadToEnd();
                            MessageReport message = (MessageReport)Newtonsoft.Json.JsonConvert.DeserializeObject(json, this.GetType());
                            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                            foreach (PropertyInfo property in properties)
                            {
                                property.SetValue(this, property.GetValue(message, null), null);
                            }

                            this.IsLoaded = true;
                            return true;
                        }
                    }
                    
                    return false;
                }

                return false;
             }
            catch
            {
                return false;
            }
        }
    }
}
