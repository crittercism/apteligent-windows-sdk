using CrittercismSDK;
using CrittercismSDK.DataContracts.Legacy;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests {
    [TestClass]
    class ExceptionHandlersTests {
        [TestMethod]
        public void LogHandledExceptionTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            Crittercism.LeaveBreadcrumb("HandledExceptionBreadcrumb");
            Crittercism.SetValue("favoriteFood", "Texas Sheet Cake");
            TestHelpers.CleanUp(); // drop all previous messages
            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
            HandledException he = Crittercism.MessageQueue.Dequeue() as HandledException;
            he.DeleteFromDisk();
            Assert.IsNotNull(he, "Expected a HandledException message");
            String asJson = Newtonsoft.Json.JsonConvert.SerializeObject(he);
            TestHelpers.checkCommonJsonFragments(asJson);
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
    }
}
