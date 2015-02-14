// file:	CrittercismSDK\StorageHelper.cs
// summary:	Implements the storage helper class
namespace CrittercismSDK
{
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization.Json;
#if NETFX_CORE
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Windows.Foundation;
    using Windows.Storage;
    using Windows.Storage.Search;
#else
    using System.IO.IsolatedStorage;
    using System.Reflection;
#endif

    /// <summary>
    /// Storage helper.
    /// </summary>
    internal static class StorageHelper {

        internal static ulong AvailableFreeSpace() {
#if NETFX_CORE
            // TODO: Return something better than zero.
            return 0;
#else
            return (ulong)GetStore().AvailableFreeSpace;
#endif
        }

        internal static void CreateFolder(string path) {
#if NETFX_CORE
            WindowsRuntimeSystemExtensions.AsTask<StorageFolder>(
                GetStore().CreateFolderAsync(path),
                CancellationToken.None
            ).Wait();
#else
            GetStore().CreateDirectory(path);
#endif
        }

        /// <summary>
        /// Loads from disk.
        /// </summary>
        /// <param name="dataType"> Type of the data. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal static object Load(Type dataType) {
            object data=null;
            try {
                string path=dataType.Name+".js";
                data=Load(path,dataType);
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            };
            return data;
        }

        /// <summary>
        /// Load JSON deserializable data object from IsolatedStorageFile path.
        /// </summary>
        /// <param name="dataType"> Type of the data. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal static object Load(string path,Type dataType) {
            object data=null;
            try {
                Debug.WriteLine("Load: "+Path.Combine(GetStorePath(),path));
                if (FileExists(path)) {
                    string dataString=LoadString(path);
                    if (dataString!=null) {
                        data=JsonConvert.DeserializeObject(dataString,dataType);
                    } else {
                        Debug.WriteLine("Load: Unable to parse "+path);
                    }
                } else {
                    Debug.WriteLine("Load: File doesn't exist "+path);
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
            return data;
        }

        private static string LoadString(string path) {
            string dataString=null;
#if NETFX_CORE
            {
                StorageFile file=WindowsRuntimeSystemExtensions.AsTask<StorageFile>(
                    GetStore().GetFileAsync(path),
                    CancellationToken.None
                ).Result;
                dataString=(string)WindowsRuntimeSystemExtensions.AsTask(
                    FileIO.ReadTextAsync(file)
                ).Result;
                Debug.WriteLine("LoadString: "+dataString);
            }
#else
            {
                IsolatedStorageFile storage=StorageHelper.GetStore();
                using (IsolatedStorageFileStream stream=storage.OpenFile(path,FileMode.Open,FileAccess.Read)) {
                    StreamReader reader=new StreamReader(stream);
                    dataString=reader.ReadToEnd();
                }
            }
#endif
            return dataString;
        }

        /// <summary>
        /// Saves to disk.
        /// </summary>
        /// <param name="Data"> The data. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal static bool Save(object data) {
            bool answer=false;
            try {
                string path=data.GetType().Name+".js";
                answer=Save(data,path);
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            };
            return answer;
        }

        /// <summary>
        /// Save JSON serializable data object to disk.
        /// </summary>
        /// <param name="Data"> The data. </param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        internal static bool Save(object data,string path) {
            bool answer=false;
            try {
                Debug.WriteLine("Save: "+Path.Combine(GetStorePath(),path));
                string dataString=JsonConvert.SerializeObject(data);
                SaveString(path,dataString);
                answer=true;
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            };
            return answer;
        }

#if NETFX_CORE
        private static StorageFile TryGetFile(string path) {
            //Debug.WriteLine("TryGetFile: "+path);
            StorageFile answer=null;
#if WINDOWS_APP
            {
                IStorageItem item=WindowsRuntimeSystemExtensions.AsTask<IStorageItem>(
                    GetStore().TryGetItemAsync(path),
                    CancellationToken.None
                ).Result;
                if ((item!=null)&&(item.IsOfType(StorageItemTypes.File))) {
                    answer=(StorageFile)item;
                }
            }
#elif true //WINDOWS_PHONE_APP
            {
                // WindowsPhoneApp StorageFolder doesn't have TryGetItemAsync,
                // so the code gets a little messier on this platform.  Researched
                // various online advice, such as:
                // http://suchan.cz/2014/07/file-io-best-practices-in-windows-and-phone-apps-part-1-available-apis-and-file-exists-checking/
                // http://blogs.msdn.com/b/shashankyerramilli/archive/2014/02/17/check-if-a-file-exists-in-windows-phone-8-and-winrt-without-exception.aspx
                // Decided we do not want Crittercism SDK to be seen throwing
                // 'System.AggregateException' wrapping 'System.IO.FileNotFoundException'
                // visible in Crittercism user's VS2013 "Output" console.  Iteration 
                // avoids this, is good enough for MSDN blogger to consider it, and
                // we like to think the Crittercism SDK actually will not be calling
                // this method very much.
                String directoryName=Path.GetDirectoryName(path);
                StorageFolder store=TryGetFolder(directoryName);
                if (store!=null) {
                    String fileName=Path.GetFileName(path);
                    IReadOnlyList<StorageFile> files=WindowsRuntimeSystemExtensions.AsTask<IReadOnlyList<StorageFile>>(
                         store.GetFilesAsync(),
                         CancellationToken.None
                    ).Result;
                    foreach (StorageFile file in files) {
                        if (file.Name.Equals(fileName,StringComparison.OrdinalIgnoreCase)) {
                            answer=file;
                            break;
                        }
                    }
                }
            }
#endif
            //Debug.WriteLine("TryGetFile: ---> "+answer);
            return answer;
        }
#endif // NETFX_CORE

#if NETFX_CORE
        private static StorageFolder TryGetFolder(string path) {
            StorageFolder answer=null;
#if WINDOWS_APP
            {
                IStorageItem item=WindowsRuntimeSystemExtensions.AsTask<IStorageItem>(
                    GetStore().TryGetItemAsync(path),
                    CancellationToken.None
                ).Result;
                if ((item!=null)&&(item.IsOfType(StorageItemTypes.Folder))) {
                    answer=(StorageFolder)item;
                }
            }
#elif true //WINDOWS_PHONE_APP
            {
                // WindowsPhoneApp StorageFolder doesn't have TryGetItemAsync .
                // Navigate to the end of the path.
                answer=GetStore();
                if (!path.Equals("")) {
                    String directoryName=Path.GetDirectoryName(path);
                    if (!directoryName.Equals("")) {
                        answer=TryGetFolder(directoryName);
                    }
                    String fileName=Path.GetFileName(path);
                    if (!fileName.Equals("")) {
                        IReadOnlyList<StorageFolder> folders=WindowsRuntimeSystemExtensions.AsTask<IReadOnlyList<StorageFolder>>(
                             answer.GetFoldersAsync(),
                             CancellationToken.None
                        ).Result;
                        answer=null;
                        foreach (StorageFolder folder in folders) {
                            if (folder.Name.Equals(path,StringComparison.OrdinalIgnoreCase)) {
                                answer=folder;
                                break;
                            }
                        }
                    }
                }
            }
#endif
            return answer;
        }
#endif // NETFX_CORE


#if NETFX_CORE
        private static StorageFile SaveFile(string path) {
            // file = fopen(path,"w");
            StorageFile file=TryGetFile(path);
            if (file==null) {
                file=WindowsRuntimeSystemExtensions.AsTask<StorageFile>(
                    GetStore().CreateFileAsync(path),
                    CancellationToken.None
                ).Result;
            }
            return file;
        }
#endif

        private static void SaveString(string path,string dataString) {
#if NETFX_CORE
            {
                StorageFile file=SaveFile(path);
                Debug.WriteLine("SaveString: "+dataString);
                WindowsRuntimeSystemExtensions.AsTask(
                    FileIO.WriteTextAsync(file,dataString)
                ).Wait();
            }
#else
            {
                IsolatedStorageFile storage=StorageHelper.GetStore();
                using (IsolatedStorageFileStream stream=new IsolatedStorageFileStream(path,FileMode.Create,FileAccess.Write,storage)) {
                    StreamWriter writer=new StreamWriter(stream);
                    writer.Write(dataString);
                    writer.Flush();
                    writer.Close();
                }
            }
#endif
        }

        internal static void DeleteFile(string path) {
#if NETFX_CORE
            {
                StorageFile file=WindowsRuntimeSystemExtensions.AsTask<StorageFile>(
                    GetStore().GetFileAsync(path),
                    CancellationToken.None
                ).Result;
                WindowsRuntimeSystemExtensions.AsTask(
                    file.DeleteAsync(StorageDeleteOption.Default)
                ).Wait();
            }
#else
            GetStore().DeleteFile(path);
#endif
        }

        internal static bool FileExists(string path) {
            //Debug.WriteLine("FileExists: "+path);
            bool answer=false;
#if NETFX_CORE
            try {
                StorageFile file=TryGetFile(path);
                if (file!=null) {
                    //Debug.WriteLine("FileExists: answer=true");
                    answer=true;
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
#else
            answer=GetStore().FileExists(path);
#endif
            //Debug.WriteLine("FileExists --> "+answer);
            return answer;
        }

        internal static bool FolderExists(string path) {
            //Debug.WriteLine("FolderExists: "+path);
            bool answer=false;
#if NETFX_CORE
            try {
                StorageFolder folder=TryGetFolder(path);
                if (folder!=null) {
                    //Debug.WriteLine("FolderExists: answer=true");
                    answer=true;
                }
            } catch (Exception e) {
                Crittercism.LogInternalException(e);
            }
#else
            answer=GetStore().DirectoryExists(path);
#endif
            //Debug.WriteLine("FolderExists --> "+answer);
            return answer;
        }

#if NETFX_CORE
        internal static StorageFolder GetStore() {
            StorageFolder storage=ApplicationData.Current.LocalFolder;
            return storage;
        }
#else
        private static IsolatedStorageFile GetStore() {
#if WINDOWS_PHONE
            IsolatedStorageFile storage=IsolatedStorageFile.GetUserStoreForApplication();
#else
            IsolatedStorageFile storage=IsolatedStorageFile.GetUserStoreForAssembly();
#endif // WINDOWS_PHONE
            return storage;
        }
#endif // NETFX_CORE

        internal static string GetStorePath() {
#if NETFX_CORE
            return GetStore().Path;
#else
            // TODO: Can't we do better than this?
            return "";
#endif
        }

        internal static DateTimeOffset GetCreationTime(string path) {
#if NETFX_CORE
            {
                StorageFile file=WindowsRuntimeSystemExtensions.AsTask<StorageFile>(
                    GetStore().GetFileAsync(path),
                    CancellationToken.None
                ).Result;
                return file.DateCreated;
            }
#else
            return GetStore().GetCreationTime(path);
#endif
        }

        internal static string[] GetFileNames(string path) {
            // path == path to folder
#if NETFX_CORE
            {
                StorageFolder folder=WindowsRuntimeSystemExtensions.AsTask<StorageFolder>(
                     GetStore().GetFolderAsync(path),
                     CancellationToken.None
                ).Result;
                IReadOnlyList<StorageFile> files=WindowsRuntimeSystemExtensions.AsTask<IReadOnlyList<StorageFile>>(
                     folder.GetFilesAsync(),
                     CancellationToken.None
                ).Result;
                List<string> list=new List<string>();
                foreach (StorageFile file in files) {
                    list.Add(file.Name);
                }
                return list.ToArray();
            }
#else
            return GetStore().GetFileNames(path+"\\*");
#endif
        }


    }
}
