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
        [TestMethod]
        public void CrashLoadAfterSaveTest() {
            TestHelpers.StartApp();
            TestHelpers.LogUnhandledException();
            Crash crash=TestHelpers.DequeueMessageType(typeof(Crash)) as Crash;
            String expectedJson=JsonConvert.SerializeObject(crash);
            crash.Save();
            Crash loadedCrash=(Crash)MessageReport.LoadMessage(crash.Name);
            var loadedJson=JsonConvert.SerializeObject(loadedCrash);
            Assert.AreEqual(loadedJson,expectedJson);
            // Since crash and loadedCrash have same Name , this Delete
            // deletes the persisted record of both objects.
            loadedCrash.Delete();
            Assert.IsNull(MessageReport.LoadMessage(crash.Name));
        }

        [TestMethod]
        public void CrashHasExpectedDataTest() {
            TestHelpers.StartApp();
            TestHelpers.LogUnhandledException();
            Crash crash=TestHelpers.DequeueMessageType(typeof(Crash)) as Crash;
            Debug.WriteLine("crash.crash.name == "+crash.crash.name);
            Debug.WriteLine("crash.crash.reason == "+crash.crash.reason);
            Debug.WriteLine("crash.crash.stack_trace.Count == "+crash.crash.stack_trace.Count);
            Debug.WriteLine("crash.crash.stack_trace[0] == "+crash.crash.stack_trace[0]);
            Debug.WriteLine("crash.platform.device_id == "+crash.platform.device_id);
            Debug.WriteLine("crash.platform.device_model == "+crash.platform.device_model);
            Debug.WriteLine("crash.platform.os_name == "+crash.platform.os_name);
            Assert.AreEqual(crash.app_id,TestHelpers.VALID_APPID);
            Assert.AreEqual(crash.crash.name,"System.DivideByZeroException");
            Assert.AreEqual(crash.crash.reason,"Attempted to divide by zero.");
            Assert.AreEqual(crash.crash.stack_trace.Count,3);
            Assert.IsTrue(crash.crash.stack_trace[0].IndexOf("System.DivideByZeroException")>=0);
            Assert.IsTrue(crash.crash.stack_trace[0].IndexOf("Attempted to divide by zero.")>=0);
            Assert.IsNotNull(crash.platform.device_id);
            Assert.AreEqual(crash.platform.device_model,"Windows PC");
            Assert.AreEqual(crash.platform.os_name,Crittercism.OSName);
        }
    }
}
