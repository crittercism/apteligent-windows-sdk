using CrittercismSDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest {
    [TestClass]
    public class CrashTests {
        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            Crittercism.Shutdown();
            TestHelpers.Cleanup();
        }
        [TestMethod]
        public void CrashReportJsonTest() {
            TestHelpers.StartApp();
            // This is a simple test that just creates a CrashReport in memory
            // and tries to serialize and deserialize it completely independently
            // of the MessageQueue code.
            // Create crashReport .
            string appId = TestHelpers.VALID_APPID;
            Dictionary<string,string> metadata = new Dictionary<string,string>();
            List<UserBreadcrumb> previous_session = new List<UserBreadcrumb>();
            List<UserBreadcrumb> current_session = new List<UserBreadcrumb>();
            UserBreadcrumbs breadcrumbs = new UserBreadcrumbs(previous_session,current_session);
            List<Endpoint> endpoints = new List<Endpoint>();
            List<Breadcrumb> systemBreadcrumbs = new List<Breadcrumb>();
            List<UserFlow> userFlows = new List<UserFlow>();
            ExceptionObject exception = new ExceptionObject("name","reason","stackframe1\r\nstackframe2");
            CrashReport crashReport1 = new CrashReport(appId,metadata,breadcrumbs,endpoints,systemBreadcrumbs,userFlows,exception);
            // Testing EndpointConverter WriteJson
            string json1 = JsonConvert.SerializeObject(crashReport1);
            Assert.IsTrue(json1.IndexOf("\"app_id\":") >= 0);
            Assert.IsTrue(json1.IndexOf("\"app_state\":") >= 0);
            Assert.IsTrue(json1.IndexOf("\"breadcrumbs\":{") >= 0);
            Assert.IsTrue(json1.IndexOf("\"endpoints\":[],") >= 0);
            Assert.IsTrue(json1.IndexOf("\"systemBreadcrumbs\":[],") >= 0);
            Assert.IsTrue(json1.IndexOf("\"transactions\":[],") >= 0);
            Assert.IsTrue(json1.IndexOf("\"metadata\":{},") >= 0);
            Assert.IsTrue(json1.IndexOf("\"crash\":{") >= 0);
            Assert.IsTrue(json1.IndexOf("\"platform\":") >= 0);
            // Testing CrashReportConverter ReadJson
            CrashReport crashReport2 = JsonConvert.DeserializeObject(json1,typeof(CrashReport)) as CrashReport;
            Assert.IsNotNull(crashReport2);
            string json2 = JsonConvert.SerializeObject(crashReport2);
            Debug.WriteLine("json1 == " + json1);
            Debug.WriteLine("json2 == " + json2);
            Assert.AreEqual(json1,json2);
        }
        [TestMethod]
        public void CrashLoadAfterSaveTest() {
            // Crash the app
            TestHelpers.StartApp();
            TestHelpers.LogUnhandledException();
            // Restart the app
            TestHelpers.StartApp();
            CrashReport crashReport = TestHelpers.DequeueMessageType(typeof(CrashReport)) as CrashReport;
            String expectedJson = JsonConvert.SerializeObject(crashReport);
            crashReport.Save();
            CrashReport loadedCrash = (CrashReport)MessageReport.LoadMessage(crashReport.Name);
            var loadedJson = JsonConvert.SerializeObject(loadedCrash);
            Assert.AreEqual(loadedJson,expectedJson);
            // Since crash and loadedCrash have same Name , this Delete
            // deletes the persisted record of both objects.
            loadedCrash.Delete();
            Assert.IsNull(MessageReport.LoadMessage(crashReport.Name));
        }

        [TestMethod]
        public void CrashHasExpectedDataTest() {
            // Crash the app
            TestHelpers.StartApp();
            TestHelpers.LogUnhandledException();
            // Restart the app
            TestHelpers.StartApp();
            CrashReport crashReport = TestHelpers.DequeueMessageType(typeof(CrashReport)) as CrashReport;
            Trace.WriteLine("crashReport.crash.name == " + crashReport.crash.name);
            Trace.WriteLine("crashReport.crash.reason == " + crashReport.crash.reason);
            Trace.WriteLine("crashReport.crash.stack_trace.Count == " + crashReport.crash.stack_trace.Count);
            Trace.WriteLine("crashReport.crash.stack_trace[0] == " + crashReport.crash.stack_trace[0]);
            Trace.WriteLine("crashReport.platform.device_id == " + crashReport.platform.device_id);
            Trace.WriteLine("crashReport.platform.device_model == " + crashReport.platform.device_model);
            Trace.WriteLine("crashReport.platform.os_name == " + crashReport.platform.os_name);
            Assert.AreEqual(crashReport.app_id,TestHelpers.VALID_APPID);
            Assert.AreEqual(crashReport.crash.name,"System.DivideByZeroException");
            Assert.AreEqual(crashReport.crash.reason,"Attempted to divide by zero.");
            // NOTE: crashReport.crash.stack_trace.Count is smaller in "Release" build.
            Assert.IsTrue(crashReport.crash.stack_trace.Count <= 3);
            Assert.IsTrue(crashReport.crash.stack_trace.Count >= 2);
            Assert.IsTrue(crashReport.crash.stack_trace[0].IndexOf("System.DivideByZeroException") >= 0);
            Assert.IsTrue(crashReport.crash.stack_trace[0].IndexOf("Attempted to divide by zero.") >= 0);
            Assert.IsNotNull(crashReport.platform.device_id);
            Assert.AreEqual(crashReport.platform.device_model,"Windows PC");
            Assert.AreEqual(crashReport.platform.os_name,Crittercism.OSName);
        }
    }
}
