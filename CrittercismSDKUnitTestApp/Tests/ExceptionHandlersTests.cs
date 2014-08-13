using CrittercismSDK;
using CrittercismSDK.DataContracts;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests {
    [TestClass]
    public class ExceptionHandlersTests {
        [TestMethod]
        public void LogHandledExceptionTest() {
            Crittercism._autoRunQueueReader = false;
            TestHelpers.InitializeRemoveLoadFromQueue(TestHelpers.VALID_APPID);

            Crittercism.LeaveBreadcrumb("HandledExceptionBreadcrumb");
            Crittercism.SetValue("favoriteFood", "Texas Sheet Cake");
            TestHelpers.CleanUp(); // drop all previous messages

            try {
                TestHelpers.ThrowDivideByZeroException();
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }

            HandledException he = Crittercism.MessageQueue.Dequeue() as HandledException;
            he.DeleteFromDisk();
            Assert.IsNotNull(he, "Expected a HandledException message");

            String asJson = Newtonsoft.Json.JsonConvert.SerializeObject(he);
            TestHelpers.CheckCommonJsonFragments(asJson);
            string[] jsonStrings = new string[] {
                "\"breadcrumbs\":",
                "\"current_session\":[{\"message\":\"HandledExceptionBreadcrumb\"",
                "\"metadata\":{",
                "\"favoriteFood\":\"Texas Sheet Cake\"}",
            };
            foreach (String jsonFragment in jsonStrings) {
                Assert.IsTrue(asJson.Contains(jsonFragment));
            }
        }

        // TODO(DA) Specify better behavior here. What should this do?
        public void HandledExceptionWithNullStackTraceOK() {
            var ex = new Exception();
            Crittercism.LogHandledException(ex);
        }
    }
}
