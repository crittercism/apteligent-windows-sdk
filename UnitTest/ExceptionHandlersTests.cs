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
    public class ExceptionHandlersTests {
        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            Crittercism.Shutdown();
            TestHelpers.Cleanup();
        }
        [TestMethod]
        public void LogHandledExceptionTest() {
            TestHelpers.StartApp();
            Crittercism.LeaveBreadcrumb("HandledExceptionBreadcrumb");
            Crittercism.SetValue("favoriteFood","Texas Sheet Cake");
            TestHelpers.Cleanup(); // drop all previous messages
            TestHelpers.LogHandledException();
            MessageReport messageReport = TestHelpers.DequeueMessageType(typeof(HandledException));
            Assert.IsNotNull(messageReport,"Expected a HandledException message");
            String asJson = JsonConvert.SerializeObject(messageReport);
            Trace.WriteLine("asJson == " + asJson);
            TestHelpers.CheckCommonJsonFragments(asJson);
            string[] jsonStrings = new string[] {
                "\"breadcrumbs\":",
                "\"current_session\":",
                "\"message\":\"session_start\"",
                "\"metadata\":{",
                "\"favoriteFood\":\"Texas Sheet Cake\""
            };
            foreach (String jsonFragment in jsonStrings) {
                Trace.WriteLine("jsonFragment == " + jsonFragment);
                Trace.WriteLine("asJson.Contains(jsonFragment) == " + asJson.Contains(jsonFragment));
                Assert.IsTrue(asJson.Contains(jsonFragment));
            };
        }

        [TestMethod]
        public void HandledUnthrownExceptionTest() {
            try {
                Exception exception = new Exception("description");
                exception.Data.Add("MethodName","methodName");
                Crittercism.LogHandledException(exception);
            } catch (Exception) {
                // logHandledException should not throw its own exception
                // when passed an unthrown user created Exception object
                // with null stacktrace.
                Assert.Fail();
            }
        }
    }
}
