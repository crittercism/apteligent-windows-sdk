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
    class MiscTests {
        [TestMethod]
        public void TruncatedBreadcrumbTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            TestHelpers.CleanUp(); // drop all previous messages
            // start breadcrumb with sentinel to ensure we don't left-truncate
            string breadcrumb = "raaaaaaaaa";
            for (int x = 0; x < 13; x++) {
                breadcrumb += "aaaaaaaaaa";
            }
            // end breadcrumb with "illegal" chars and check for their presence
            breadcrumb += "zzzzzzzzzz";
            Crittercism.LeaveBreadcrumb(breadcrumb);
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
            Assert.IsTrue(asJson.Contains("\"breadcrumbs\":"));
            Assert.IsTrue(asJson.Contains("\"raaaaaa"));
            Assert.IsFalse(asJson.Contains("aaaaz"));
            Assert.IsFalse(asJson.Contains("zzz"));
        }

        [TestMethod]
        public void OptOutTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            TestHelpers.CleanUp();
            Assert.IsTrue(Crittercism.MessageQueue == null || Crittercism.MessageQueue.Count == 0);
            Crittercism.SetOptOutStatus(true);
            Assert.IsTrue(Crittercism.CheckOptOutFromDisk());
            Assert.IsTrue(Crittercism.GetOptOutStatus());
            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
            Assert.IsTrue(Crittercism.MessageQueue == null || Crittercism.MessageQueue.Count == 0);
            // Now turn it back on
            Crittercism.SetOptOutStatus(false);
            Assert.IsFalse(Crittercism.CheckOptOutFromDisk());
            Assert.IsFalse(Crittercism.GetOptOutStatus());
            try {
                int k = j / i;
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
            }
            Assert.IsTrue(Crittercism.MessageQueue.Count == 1);

        }
    }
}
