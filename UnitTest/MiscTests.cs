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
        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            Crittercism.Shutdown();
            TestHelpers.Cleanup();
        }
        [TestMethod]
        public void TruncatedBreadcrumbTest() {
            TestHelpers.StartApp();
            string breadcrumb;
            {
                StringBuilder builder = new StringBuilder();
                // start breadcrumb with sentinel to ensure we don't left-truncate
                builder.Append("r");
                for (int i=1;i<Breadcrumbs.MAX_TEXT_LENGTH;i++) {
                    builder.Append("a");
                };
                // end breadcrumb with "illegal" chars and check for their presence
                builder.Append("zzzzzzzzzz");
                breadcrumb = builder.ToString();
            };
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
            Trace.WriteLine("Crittercism.MessageQueue == "+Crittercism.MessageQueue);
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
