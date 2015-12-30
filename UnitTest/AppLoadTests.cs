using CrittercismSDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest {
    [TestClass]
    public class AppLoadTests {
        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            Crittercism.Shutdown();
            TestHelpers.Cleanup();
        }
        [TestMethod]
        public void AppLoadTest1() {
            // Test AppLoad json message that is sent.
            TestHelpers.StartApp();
            MessageReport messageReport = TestHelpers.DequeueMessageType(typeof(AppLoad));
            Assert.IsNotNull(messageReport,"Expected an AppLoad message");
            string json = JsonConvert.SerializeObject(messageReport);
            TestHelpers.CheckJson(json);
        }
        [TestMethod]
        public void AppLoadTest2() {
            // Test the AppLoad json response equals ExampleResponse.
            {
                // Invalidate current Crittercism.Settings .
                Crittercism.Settings = null;
            }
            // StartApp
            TestHelpers.StartApp();
            // Wait sufficiently long for AppLoad response.
            TestHelpers.DequeueMessageType(typeof(AppLoad));
            {
                // Check Crittercism.Settings agrees with ExampleResponse.
                Assert.IsNotNull(Crittercism.Settings);
                Assert.IsTrue(Crittercism.Settings is JObject);
                JObject settings = Crittercism.Settings;
                Assert.IsTrue(Crittercism.CheckSettings(settings));
            }
            {
                // Check Crittercism.Settings Details.
                JObject config = Crittercism.Settings["txnConfig"] as JObject;
                if (config["enabled"] != null) {
                    bool enabled = (bool)((JValue)(config["enabled"])).Value;
                    Assert.IsTrue(enabled);
                    int interval = Convert.ToInt32(((JValue)(config["interval"])).Value);
                    Assert.IsTrue(interval == 10);
                    int defaultTimeout = Convert.ToInt32(((JValue)(config["defaultTimeout"])).Value);
                    Assert.IsTrue(defaultTimeout == 3600000);
                    JObject thresholds = config["transactions"] as JObject;
                    Assert.IsNotNull(thresholds);
                    JObject threshold = thresholds["Buy Critter Feed"] as JObject;
                    Assert.IsNotNull(threshold);
                    int timeout = Convert.ToInt32(((JValue)(threshold["timeout"])).Value);
                    Assert.IsTrue(timeout == 60000);
                    int value = Convert.ToInt32(((JValue)(threshold["value"])).Value);
                    Assert.IsTrue(value == 1299);
                }
            }
        }
    }
}
