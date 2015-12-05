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
        public void CrashLoadAfterSaveTest() {
            TestHelpers.StartApp();
            TestHelpers.LogUnhandledException();
            CrashReport crashReport =TestHelpers.DequeueMessageType(typeof(CrashReport)) as CrashReport;
            String expectedJson=JsonConvert.SerializeObject(crashReport);
            crashReport.Save();
            CrashReport loadedCrash =(CrashReport)MessageReport.LoadMessage(crashReport.Name);
            var loadedJson=JsonConvert.SerializeObject(loadedCrash);
            Assert.AreEqual(loadedJson,expectedJson);
            // Since crash and loadedCrash have same Name , this Delete
            // deletes the persisted record of both objects.
            loadedCrash.Delete();
            Assert.IsNull(MessageReport.LoadMessage(crashReport.Name));
        }

        [TestMethod]
        public void CrashHasExpectedDataTest() {
            TestHelpers.StartApp();
            TestHelpers.LogUnhandledException();
            CrashReport crashReport=TestHelpers.DequeueMessageType(typeof(CrashReport)) as CrashReport;
            Trace.WriteLine("crashReport.crash.name == "+crashReport.crash.name);
            Trace.WriteLine("crashReport.crash.reason == "+crashReport.crash.reason);
            Trace.WriteLine("crashReport.crash.stack_trace.Count == "+crashReport.crash.stack_trace.Count);
            Trace.WriteLine("crashReport.crash.stack_trace[0] == "+crashReport.crash.stack_trace[0]);
            Trace.WriteLine("crashReport.platform.device_id == "+crashReport.platform.device_id);
            Trace.WriteLine("crashReport.platform.device_model == "+crashReport.platform.device_model);
            Trace.WriteLine("crashReport.platform.os_name == "+crashReport.platform.os_name);
            Assert.AreEqual(crashReport.app_id,TestHelpers.VALID_APPID);
            Assert.AreEqual(crashReport.crash.name,"System.DivideByZeroException");
            Assert.AreEqual(crashReport.crash.reason,"Attempted to divide by zero.");
            // NOTE: crashReport.crash.stack_trace.Count is smaller in "Release" build.
            Assert.IsTrue(crashReport.crash.stack_trace.Count<=3);
            Assert.IsTrue(crashReport.crash.stack_trace.Count>=2);
            Assert.IsTrue(crashReport.crash.stack_trace[0].IndexOf("System.DivideByZeroException")>=0);
            Assert.IsTrue(crashReport.crash.stack_trace[0].IndexOf("Attempted to divide by zero.")>=0);
            Assert.IsNotNull(crashReport.platform.device_id);
            Assert.AreEqual(crashReport.platform.device_model,"Windows PC");
            Assert.AreEqual(crashReport.platform.os_name,Crittercism.OSName);
        }
    }
}
