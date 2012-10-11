// file:	CrittercismSDK\MessageReport.cs
// summary:	Implements the message report class
namespace CrittercismSDK.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using Windows.Storage;

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
            bool result = false;
            try
            {
                Name = this.GetType().Name + "_" + Guid.NewGuid().ToString() + ".txt";
                result = StorageHelper.SaveToDisk(this, Crittercism.messageFolder, this.Name);
                this.IsLoaded = result;
                return result;
            }
            catch
            {
                return result;
            }
        }

        /// <summary>
        /// Deletes the message from disk.
        /// </summary>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal bool DeleteFromDisk()
        {
            try
            {
                return StorageHelper.DeleteFromDisk(Crittercism.messageFolder, this.Name);
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
            try
            {
                MessageReport message = (MessageReport)StorageHelper.LoadFromDisk(this.GetType(), Crittercism.messageFolder, this.Name);
                PropertyInfo[] properties = this.GetType().GetRuntimeProperties().ToArray();
                foreach (PropertyInfo property in properties)
                {
                    if (property.Name != "Name" && property.Name != "CreationDate" && property.Name != "IsLoaded")
                    {
                        property.SetValue(this, property.GetValue(message, null), null);
                    }
                }

                this.IsLoaded = true;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
