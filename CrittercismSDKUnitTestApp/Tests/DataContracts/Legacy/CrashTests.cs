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
        private Crash GetCrashMessage() {
            // A stricter language would detect that this code is unreachable.
            // I think because we allow global variable mutation to i and j?
            var crEx = new ExceptionObject("Unreachable", "Unreachable",
                "Unreachable");
            
            try {
                int i = 1;
                int j = 0;
                var k = i / j;

                // This is unreachable; above always throws divide-by-zero
                return new Crash(TestHelpers.VALID_APPID, System.Windows.Application.Current.
                    GetType().Assembly.GetName().Version.ToString(),
                    new Dictionary<string, string>(), new Breadcrumbs(), crEx);
            } catch(Exception ex) {
                crEx = new ExceptionObject(ex.GetType().FullName, ex.Message, ex.StackTrace);
                return new Crash(TestHelpers.VALID_APPID, System.Windows.
                    Application.Current.GetType().Assembly.GetName().Version.ToString(),
                    new Dictionary<string, string>(), new Breadcrumbs(), crEx);
            }
        }
        
        [TestMethod]
        public void CrashDiskRoundTripTest() {
            var crash = GetCrashMessage();
            string expectedJson = Newtonsoft.Json.JsonConvert.SerializeObject(crash);

            try {
                crash.SaveToDisk();

                Crash messageReportLoaded = new Crash {
                    Name = crash.Name
                };
                messageReportLoaded.LoadFromDisk();
                var actualJson = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);

                Assert.AreEqual(expectedJson, actualJson);
            } finally {
                crash.DeleteFromDisk();
            }
        }

        [TestMethod]
        public void CrashFormatTest() {
            TestHelpers.InitializeRemoveLoadFromQueue(TestHelpers.VALID_APPID);
            var crash = GetCrashMessage();

            Assert.AreEqual(crash.app_id, TestHelpers.VALID_APPID);
            Assert.AreEqual(crash.crash.name, "System.DivideByZeroException");
            Assert.AreEqual(crash.crash.reason, "Attempted to divide by zero.");
            Assert.AreEqual(crash.crash.stack_trace.Count, 1);
            Assert.AreEqual(crash.crash.stack_trace[0], "   at CrittercismSDKUnitTestApp.Tests.DataContracts.Legacy.CrashTests.GetCrashMessage()");
            Assert.AreEqual(crash.platform.client, "wp8v2.0");
            Assert.IsNotNull(crash.platform.device_id);
            Assert.AreEqual(crash.platform.device_model, "XDeviceEmulator");
            Assert.AreEqual(crash.platform.os_name, "wp");
        }
        
        [TestMethod]
        public void CreateCrashReportTest() {
            TestHelpers.InitializeLeaveLoadOnQueue(TestHelpers.VALID_APPID);
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
            TestHelpers.InitializeLeaveLoadOnQueue(TestHelpers.VALID_APPID);
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;

            try {
                TestHelpers.ThrowDivideByZeroException();
            } catch (Exception ex) {
                Crittercism.LeaveBreadcrumb("Breadcrum test");
                Crittercism.CreateCrashReport(ex);
                QueueReader queueReader = new QueueReader();

                // TODO(DA): Assert.DoesNotThrow()...need a better assertion here
                queueReader.ReadQueue();
            }
        }
    }
}
