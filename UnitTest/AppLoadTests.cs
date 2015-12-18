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
                string json = JsonConvert.SerializeObject(messageReport);
                TestHelpers.CheckJson(json);
            } finally {
                Crittercism.Shutdown();
                TestHelpers.Cleanup();
            }
        }
    }
}
