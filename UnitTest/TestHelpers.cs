using CrittercismSDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest {
    class TestHelpers {
        public const string VALID_APPID = "50807ba33a47481dd5000002";

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context) {
            Cleanup();
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup() {
            Crittercism.Shutdown();
        }

        public static void CheckCommonJsonFragments(String json) {
            Platform p = new Platform();
            string[] jsonStrings = new string[] {
                "\"app_id\":\"50807ba33a47481dd5000002\"",
                "\"app_state\":{\"app_version\":\"",
                "\"platform\":{\"client\":",
                "\"device_id\":\"" + p.device_id + "\"",
                "\"device_model\":",
                "\"os_name\":",
                "\"os_version\":",
                "\"locale\":",
            };
            foreach (string jsonFragment in jsonStrings) {
                Trace.WriteLine("jsonFragment == " + jsonFragment);
                Trace.WriteLine("json.Contains == " + json.Contains(jsonFragment));
                Assert.IsTrue(json.Contains(jsonFragment));
            };
            // Make sure DateTimes are stringified in the canonical way and not in this goofy default way
            Assert.IsFalse(json.Contains("Date("));
        }

        public static void Cleanup() {
            // This method is for clean all the possible variables that may be will used by another unit test
            Crittercism.enableSendMessage = false;
            Crittercism.SetOptOutStatus(false);
            if (Crittercism.MessageQueue != null) {
                Crittercism.MessageQueue.Clear();
            }
            // Some unit tests might pollute the message folder.  clean those up
            try {
                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                foreach (string file in storage.GetFileNames("")) {
                    storage.DeleteFile(file);
                }
            } catch (Exception ex) {
                Console.WriteLine("Cleanup exception: " + ex);
            }
        }

        public static void StartApp(bool optOutStatus) {
            // Convenient for the OptOutTest which must pass optOutStatus = true
            Crittercism.SetOptOutStatus(optOutStatus);
            Crittercism.enableSendMessage = false;
            Crittercism.Init(VALID_APPID);
        }
        public static void StartApp() {
            // The preferred default is optOutStatus = false
            StartApp(false);
        }

        public static void ThrowException() {
            int i = 0;
            int j = 5;
            int k = j / i;
        }

        public static void LogHandledException() {
            try {
                TestHelpers.ThrowException();
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
        }

        public static void LogUnhandledException() {
            try {
                TestHelpers.ThrowException();
            } catch (Exception ex) {
                Crittercism.LogUnhandledException(ex);
            }
        }

        public static MessageReport DequeueMessageType(Type type) {
            MessageReport answer = null;
            while (Crittercism.MessageQueue.Count > 0) {
                MessageReport messageReport = Crittercism.MessageQueue.Dequeue();
                messageReport.Delete();
                if ((messageReport.GetType() == type)
                    || (messageReport.GetType().IsSubclassOf(type))) {
                    answer = messageReport;
                    break;
                }
            }
            return answer;
        }
    }
}
