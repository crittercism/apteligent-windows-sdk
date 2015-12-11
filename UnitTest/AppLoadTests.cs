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
    public class AppLoadTests {
        [TestMethod]
        public void AppLoadTest() {
            try {
                TestHelpers.StartApp();
                MessageReport messageReport = TestHelpers.DequeueMessageType(typeof(AppLoad));
                Assert.IsNotNull(messageReport,"Expected an AppLoad message");
                TestHelpers.CheckCommonJsonFragments(JsonConvert.SerializeObject(messageReport));
            } finally {
                Crittercism.Shutdown();
                TestHelpers.Cleanup();
            }
        }

#if false
        ////////////////////////////////////////////////////////////////
        // TODO: UnfifiedAppLoad.cs is some D.A. code that was never
        // completely put into production.  Currently dead weight.
        // It might be deleted in the future.
        ////////////////////////////////////////////////////////////////
        [TestMethod]
        public void AppLoadFormat() {
            UnifiedAppLoad newMessageReport = new UnifiedAppLoad(TestHelpers.VALID_APPID);
            UnifiedAppLoadInner inner = newMessageReport.appLoads;
            Assert.AreEqual(newMessageReport.count, 1);
            Assert.AreEqual(newMessageReport.current, true);
            Assert.AreEqual(inner.osName,Crittercism.OSName);
            Assert.AreEqual(inner.carrier,"UNKNOWN");     // On emulator
        }
#endif
    }
}
