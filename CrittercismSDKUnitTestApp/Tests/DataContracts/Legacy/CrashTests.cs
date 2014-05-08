using CrittercismSDK;
using CrittercismSDK.DataContracts.Legacy;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests.DataContracts.Legacy {
    [TestClass]
    public class CrashTests {
        [TestMethod]
        public void CrashDataContractTest() {
            int i = 0;
            int j = 5;
            Crash newMessageReport = null;
            string errorName = string.Empty;
            string errorMessage = string.Empty;
            string errorStackTrace = string.Empty;
            try {
                try {
                    int k = j / i;
                } catch (Exception ex) {
                    // create new crash message
                    errorName = ex.GetType().FullName;
                    errorMessage = ex.Message;
                    errorStackTrace = ex.StackTrace;
                    ExceptionObject exception = new ExceptionObject(errorName, errorMessage,
                        errorStackTrace);
                    newMessageReport = new Crash("50807ba33a47481dd5000002", System.Windows.
                        Application.Current.GetType().Assembly.GetName().Version.ToString(),
                        new Dictionary<string, string>(), new Breadcrumbs(), exception);
                    newMessageReport.SaveToDisk();
                }

                // check that message is saved by try loading it with the helper
                // load saved version of the crash event
                Crash messageReportLoaded = new Crash {
                    Name = newMessageReport.Name
                };
                messageReportLoaded.LoadFromDisk();

                Assert.IsNotNull(messageReportLoaded);

                // validate that the loaded object is corrected agains the original one via json serialization
                string originalJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport);
                string loadedJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);

                Assert.AreEqual(loadedJsonMessage, originalJsonMessage);

                // compare against known json to verify that the serialization is in the correct format
                TestHelpers.CheckCommonJsonFragments(loadedJsonMessage);
                string[] jsonStrings = new string[] {
                    "\"breadcrumbs\":{\"current_session\":[],\"previous_session\":[]}",
                    "\"crash\":{\"name\":\"" + errorName + "\",\"reason\":\"" + errorMessage + "\",\"stack_trace\":[\"" + errorStackTrace.Replace(@"\", @"\\") + "\"]}",
                };
                foreach (string jsonFragment in jsonStrings) {
                    Assert.IsTrue(loadedJsonMessage.Contains(jsonFragment));
                }
            } finally {
                // delete the message from disk
                newMessageReport.DeleteFromDisk();
            }
        }

        [TestMethod]
        public void CreateCrashReportTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            Crittercism.LeaveBreadcrumb("CrashReportBreadcrumb");
            Crittercism.SetUsername("Mr. McUnitTest");
            TestHelpers.CleanUp(); // drop all previous messages
            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                Crittercism.CreateCrashReport(ex);
            }
            Crash crash = Crittercism.MessageQueue.Dequeue() as Crash;
            crash.DeleteFromDisk();
            Assert.IsNotNull(crash, "Expected a Crash message");
            String asJson = Newtonsoft.Json.JsonConvert.SerializeObject(crash);
            TestHelpers.CheckCommonJsonFragments(asJson);
            string[] jsonStrings = new string[] {
                "\"breadcrumbs\":",
                "\"current_session\":[{\"message\":\"CrashReportBreadcrumb\"",
                "\"metadata\":{",
                "\"username\":\"Mr. McUnitTest\"",
            };
            foreach (String jsonFragment in jsonStrings) {
                Assert.IsTrue(asJson.Contains(jsonFragment));
            }
        }

        [TestMethod]
        public void CrashCommunicationTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                // Create a Breadcrum
                Crittercism.LeaveBreadcrumb("Breadcrum test");

                // Create the error message
                Crittercism.CreateCrashReport(ex);

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the crash, no exception should be rise
                queueReader.ReadQueue();
            }
        }
    }
}
