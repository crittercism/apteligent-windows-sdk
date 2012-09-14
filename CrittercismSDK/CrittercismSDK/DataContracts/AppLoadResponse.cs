// file:	DataContracts\AppLoadResponse.cs
// summary:	Implements the application load response class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Reflection;

    /// <summary>
    /// Application load response.
    /// </summary>
    [DataContract]
    public class AppLoadResponse
    {
        /// <summary>
        /// Gets or sets the identifier of the application.
        /// </summary>
        /// <value> The identifier of the application. </value>
        [DataMember]
        public string app_id { get; set; }

        /// <summary>
        /// Gets or sets the device id.
        /// </summary>
        /// <value> The device id. </value>
        [DataMember]
        public string did { get; set; }

        /// <summary>
        /// Saves the device id to disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool SaveToDisk()
        {
            try
            {
                string name = "CrittercismDevice.txt";
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                using (IsolatedStorageFileStream writeFile = new IsolatedStorageFileStream(name, FileMode.CreateNew, FileAccess.Write, storage))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(this.GetType());
                    serializer.WriteObject(writeFile, this);
                    writeFile.Flush();
                    writeFile.Close();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the device Id from disk.
        /// </summary>
        /// <returns>  true if it succeeds, false if it fails. </returns>
        internal static string GetDeviceId()
        {
            try
            {
                string name = "CrittercismDevice.txt";
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.FileExists(name))
                {
                    using (IsolatedStorageFileStream readFile = new IsolatedStorageFileStream(name, FileMode.Open, FileAccess.Read, storage))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AppLoadResponse));
                        AppLoadResponse appLoadResponse = (AppLoadResponse)serializer.ReadObject(readFile);
                        return appLoadResponse.did;
                    }
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
