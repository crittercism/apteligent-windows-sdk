// file:	CrittercismSDK\StorageHelper.cs
// summary:	Implements the storage helper class
namespace CrittercismSDK
{
    using System;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.Serialization.Json;

    /// <summary>
    /// Storage helper.
    /// </summary>
    internal static class StorageHelper {
        /// <summary>
        /// Data Folder Name
        /// </summary>
        private static string dataFolder = "CrittercismData";
        // Need a prefix here to reduce chance of collisions
        private const string DEVICE_ID_KEY = "crittercism_device_id";

        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <param name="Data"> The data. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal static bool SaveToDisk(object data) {
            try {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (!storage.DirectoryExists(dataFolder)) {
                    storage.CreateDirectory(dataFolder);
                }

                using (IsolatedStorageFileStream writeFile = new IsolatedStorageFileStream(dataFolder + "\\" + data.GetType().Name + ".txt", FileMode.Create, FileAccess.Write, storage))
                {
                    ////DataContractJsonSerializer serializer = new DataContractJsonSerializer(data.GetType());
                    ////serializer.WriteObject(writeFile, data);
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    StreamWriter writer = new StreamWriter(writeFile);
                    writer.Write(json);
                    writer.Flush();
                    writer.Close();
                }

                return true;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Loads from disk.
        /// </summary>
        /// <param name="dataType"> Type of the data. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal static object LoadFromDisk(Type dataType) {
            object data = null;
            try {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.DirectoryExists(dataFolder)) {
                    if (storage.FileExists(dataFolder + "\\" + dataType.Name + ".txt")) {
                        using (IsolatedStorageFileStream readFile = storage.OpenFile(dataFolder + "\\" + dataType.Name + ".txt", FileMode.Open, FileAccess.Read)) {
                            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(dataType);
                            StreamReader reader = new StreamReader(readFile);
                            string json = reader.ReadToEnd();
                            data = Newtonsoft.Json.JsonConvert.DeserializeObject(json, dataType);
                            // data = serializer.ReadObject(readFile);
                        }
                    }
                }

                return data;
            } catch {
                return data;
            }
        }

        /// <summary>
        /// Retrieves the device id from storage.
        /// 
        /// If we don't have a device id, we create and store a new one.
        /// </summary>
        /// <returns>String with device_id, null otherwise</returns>
        public static Guid GetOrCreateDeviceId() {
            if (System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Contains(DEVICE_ID_KEY)) {
                return Guid.Parse(System.IO.IsolatedStorage.IsolatedStorageSettings.
                    ApplicationSettings[DEVICE_ID_KEY] as String);
            } else {
                Guid deviceId = Guid.NewGuid();
                System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings[DEVICE_ID_KEY]
                    = deviceId.ToString();
                return deviceId;
            }
        }
    }
}
