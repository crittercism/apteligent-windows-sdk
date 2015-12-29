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
        private static MockNetwork mockNetwork = null;
        private static MockNetwork TestNetwork() {
            if (mockNetwork == null) {
                mockNetwork = new MockNetwork();
            };
            return mockNetwork;
        }

        private static void CheckJsonContains(String json,string[] jsonStrings) {
            foreach (string jsonFragment in jsonStrings) {
                Trace.WriteLine("jsonFragment == " + jsonFragment);
                Trace.WriteLine("json.Contains == " + json.Contains(jsonFragment));
                Assert.IsTrue(json.Contains(jsonFragment));
            };
            // Make sure DateTimes are stringified in the canonical way and not in this goofy default way
            Assert.IsFalse(json.Contains("Date("));
        }
        public static void CheckJsonLegacy(String json) {
            string[] jsonStrings = new string[] {
                "\"app_id\":\"",
                "\"app_state\":{\"app_version\":\"",
                "\"platform\":{\"client\":",
                "\"device_id\":\"",
                "\"device_model\":",
                "\"os_name\":",
                "\"os_version\":",
                "\"locale\":",
            };
            CheckJsonContains(json,jsonStrings);
        }
        public static void CheckJson(String json) {
            string[] jsonStrings = new string[] {
                "\"appID\":\"",
                "\"appVersion\":\"",
                "\"crPlatform\":\"",
                "\"crVersion\":\"",
                "\"deviceID\":\"",
                "\"deviceModel\":",
                "\"locale\":",
                "\"osName\":",
                "\"osVersion\":"
            };
            CheckJsonContains(json,jsonStrings);
        }

        public static void Cleanup() {
            // This method is for clean all the possible variables that may be will used by another unit test
            TestNetwork().Cleanup();
            Crittercism.Test = TestNetwork();
            Crittercism.SetOptOutStatus(false);
            if (Crittercism.MessageQueue != null) {
                Crittercism.MessageQueue.Clear();
            }
            // Some unit tests may pollute the Crittercism directory.  Clean it up.
            try {
                string[] files = StorageHelper.GetFileNames("Crittercism");
                foreach (string file in files) {
                    StorageHelper.DeleteFile("Crittercism\\" + file);
                };
                files = StorageHelper.GetFileNames("Crittercism\\Messages");
                foreach (string file in files) {
                    StorageHelper.DeleteFile("Crittercism\\Messages\\" + file);
                };
            } catch (Exception ex) {
                Console.WriteLine("Cleanup exception: " + ex);
            }
        }

        public static void StartApp(bool optOutStatus) {
            // Convenient for the OptOutTest which must pass optOutStatus = true
            if (Crittercism.Test == null) {
                // First time being called by the test suite.
                Cleanup();
            }
            Crittercism.SetOptOutStatus(optOutStatus);
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
            MessageReport answer = TestNetwork().DequeueMessageType(type);
            return answer;
        }
    }
}
