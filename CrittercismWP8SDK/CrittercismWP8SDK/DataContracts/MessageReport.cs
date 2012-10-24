// file:	CrittercismSDK\MessageReport.cs
// summary:	Implements the message report class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Runtime.Serialization.Json;
    using System.Reflection;

    /// <summary>
    /// Message report.
    /// </summary>
    [DataContract]
    public abstract class MessageReport
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
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
