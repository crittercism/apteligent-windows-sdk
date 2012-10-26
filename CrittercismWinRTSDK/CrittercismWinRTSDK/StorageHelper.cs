// file:	CrittercismSDK\StorageHelper.cs
// summary:	Implements the storage helper class
namespace CrittercismSDK
{
    using System;
    using System.IO;
    using Windows.Storage;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using Windows.Storage.Streams;

    /// <summary>
    /// Storage helper.
    /// </summary>
    public static class StorageHelper
    {
        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <param name="data"> The data. </param>
        /// <param name="folderName"> The name of the folder. </param>
        /// <param name="fileName"> The name of the file. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public static bool SaveToDisk(object data, string folderName, string fileName)
        {
            try
            {
                StorageFolder actualFolder = GetStorageFolder(folderName);
                StorageFile file = Task.Run(async () => await actualFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting)).Result;
                using (IRandomAccessStream writeStream = Task.Run(async () => await file.OpenAsync(FileAccessMode.ReadWrite)).Result)
                {
                    Stream outStream = writeStream.AsStreamForWrite();
                    using (StreamWriter writer = new StreamWriter(outStream))
                    {
                        writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                        writer.Flush();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads from disk.
        /// </summary>
        /// <param name="dataType"> Type of the data. </param>
        /// <param name="folderName"> The name of the folder. </param>
        /// <param name="fileName"> The name of the file. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public static object LoadFromDisk(Type dataType, string folderName, string fileName)
        {
            object data = null;
            try
            {
                StorageFolder actualFolder = GetStorageFolder(folderName);
                StorageFile file = Task.Run(async () => await actualFolder.GetFileAsync(fileName)).Result;
                using (IRandomAccessStream readStream = Task.Run(async () => await file.OpenAsync(FileAccessMode.Read)).Result)
                {
                    Stream inStream = readStream.AsStreamForRead();
                    using (StreamReader reader = new StreamReader(inStream))
                    {
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject(reader.ReadToEnd(), dataType);
                    }
                }

                return data;
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// Delete from disk.
        /// </summary>
        /// <param name="folderName"> The folder name. </param>
        /// <param name="fileName"> The file name. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public static bool DeleteFromDisk(string folderName, string fileName)
        {
            try
            {
                StorageFolder folder = GetStorageFolder(folderName);
                StorageFile file = GetFileIfExists(folder, fileName);
                if (file != null)
                {
                    Task.Run(async () => await file.DeleteAsync(StorageDeleteOption.PermanentDelete));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets the storage folder.
        /// </summary>
        /// <param name="folderName"> Name of the folder. </param>
        /// <returns>   The Storage Folder. </returns>
        public static StorageFolder GetStorageFolder(string folderName)
        {
            ApplicationData appData = ApplicationData.Current;
            StorageFolder localFolder = appData.LocalFolder;
            StorageFolder storageFolder = null;

            try
            {
                storageFolder = Task.Run(async () => await localFolder.GetFolderAsync(folderName)).Result;
                return storageFolder;
            }
            catch
            {
                storageFolder = Task.Run(async () => await localFolder.CreateFolderAsync(folderName)).Result;
                return storageFolder;
            }
        }

        /// <summary>
        /// Gets the file if exists.
        /// </summary>
        /// <param name="folder"> The storage folder. </param>
        /// <param name="fileName"> The file name. </param>
        /// <returns>   The file. </returns>
        public static StorageFile GetFileIfExists(StorageFolder folder, string fileName)
        {
            try
            {
                return Task.Run(async () => await folder.GetFileAsync(fileName)).Result;
            }
            catch
            {
                return null;
            }
        }
    }
}
