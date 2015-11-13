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
    public class MiscTests {
        [TestMethod]
        public void TruncatedBreadcrumbTest() {
            TestHelpers.StartApp();
            // start breadcrumb with sentinel to ensure we don't left-truncate
            string breadcrumb = "raaaaaaaaa";
            for (int x = 0; x < 13; x++) {
                breadcrumb += "aaaaaaaaaa";
            }
            // end breadcrumb with "illegal" chars and check for their presence
            breadcrumb += "zzzzzzzzzz";
            Crittercism.LeaveBreadcrumb(breadcrumb);
            TestHelpers.LogHandledException();
            MessageReport messageReport=TestHelpers.DequeueMessageType(typeof(HandledException));
            Assert.IsNotNull(messageReport,"Expected a HandledException message");
            String asJson=JsonConvert.SerializeObject(messageReport);
            Assert.IsTrue(asJson.Contains("\"breadcrumbs\":"));
            Assert.IsTrue(asJson.Contains("\"raaaaaa"));
            Assert.IsFalse(asJson.Contains("aaaaz"));
            Assert.IsFalse(asJson.Contains("zzz"));
        }

        [TestMethod]
        public void OptOutTest() {
            // Opt out of Crittercism prior to Init .
            TestHelpers.StartApp(true);
            Assert.IsTrue(Crittercism.GetOptOutStatus());
            TestHelpers.LogHandledException();
            Debug.WriteLine("Crittercism.MessageQueue == "+Crittercism.MessageQueue);
            Assert.IsTrue((Crittercism.MessageQueue==null)||(Crittercism.MessageQueue.Count==0));
            // Opt back into Crittercism prior to Init
            TestHelpers.StartApp(false);
            Assert.IsFalse(Crittercism.GetOptOutStatus());
            TestHelpers.LogHandledException();
            Assert.IsTrue(Crittercism.MessageQueue!=null);
            Assert.IsTrue(Crittercism.MessageQueue.Count>0);
        }
    }
}
