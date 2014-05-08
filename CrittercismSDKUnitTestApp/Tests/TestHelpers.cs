using CrittercismSDK;
using CrittercismSDK.DataContracts;
using CrittercismSDK.DataContracts.Legacy;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests {
    class TestHelpers {
        public static void CheckCommonJsonFragments(String loadedJsonMessage) {
            Platform p = new Platform();
            string[] jsonStrings = new string[] {
                "\"app_id\":\"50807ba33a47481dd5000002\"",
                "\"app_state\":{\"app_version\":\"" + System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString() + "\"",
                "\"battery_level\":",
                "\"platform\":{\"client\":",
                "\"device_id\":\"" + p.device_id + "\"",
                "\"device_model\":",
                "\"os_name\":",
                "\"os_version\":",
                "\"locale\":",
            };
            foreach (string jsonFragment in jsonStrings) {
                Assert.IsTrue(loadedJsonMessage.Contains(jsonFragment));
            }
            // Make sure DateTimes are stringified in the canonical way and not in this goofy default way
            Assert.IsFalse(loadedJsonMessage.Contains("Date("));
        }

        public static void CleanUp() {
            // This method is for clean all the possible variables that may be will used by another unit test
            Crittercism._autoRunQueueReader = true;
            Crittercism._enableCommunicationLayer = true;
            Crittercism._enableRaiseExceptionInCommunicationLayer = false;
            Crittercism.OptOut = false;
            while (Crittercism.MessageQueue != null && Crittercism.MessageQueue.Count > 0) {
                MessageReport message = Crittercism.MessageQueue.Dequeue();
                message.DeleteFromDisk();
            }
            // Some unit tests might pollute the message folder.  clean those up
            string folderName = Crittercism.FolderName;
            try {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.DirectoryExists(folderName)) {
                    foreach (string file in storage.GetFileNames(folderName)) {
                        storage.DeleteFile(file);
                    }
                    storage.DeleteDirectory(folderName);
                }
            } catch (Exception ex) {
                Console.WriteLine("cleanUp exception: " + ex);
            }
        }

        public const string VALID_APPID = "50807ba33a47481dd5000002";
    }
}
