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
            Assert.IsTrue(APM.enabled);
            Assert.IsTrue(UserflowReporter.enabled);
        }
        [TestMethod]
        public void AppLoadTest2() {
            // Test the AppLoad json response equals MockNetork.DefaultAppLoadResponse .
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
        // Example AppLoad response slightly different from MockNetork.DefaultAppLoadResponse .
        private string AppLoadTest3Response = (
            "{\"txnConfig\":{\"defaultTimeout\":3600000,\n"
            + "              \"interval\":10,\n"
            + "              \"enabled\":false,\n"
            + "              \"transactions\":{\"Buy Critter Feed\":{\"timeout\":60000,\"slowness\":3600000,\"value\":1299},\n"
            + "                              \"Sing Critter Song\":{\"timeout\":90000,\"slowness\":3600000,\"value\":1500},\n"
            + "                              \"Write Critter Poem\":{\"timeout\":60000,\"slowness\":3600000,\"value\":2000}}},\n"
            + " \"apm\":{\"net\":{\"enabled\":false,\n"
            + "               \"persist\":false,\n"
            + "               \"interval\":10}},\n"
            + " \"needPkg\":1,\n"
            + " \"internalExceptionReporting\":true}"
        );
        [TestMethod]
        public void AppLoadTest3() {
            // Testing an AppLoad response which disables APM and UserflowReporter
            TestHelpers.TestNetwork().AppLoadResponse = AppLoadTest3Response;
            TestHelpers.StartApp();
            MessageReport messageReport = TestHelpers.DequeueMessageType(typeof(AppLoad));
            Assert.IsNotNull(messageReport,"Expected an AppLoad message");
            string json = JsonConvert.SerializeObject(messageReport);
            TestHelpers.CheckJson(json);
            Assert.IsFalse(APM.enabled);
            Assert.IsFalse(UserflowReporter.enabled);
            // TestHelpers.Cleanup() gets TestHelpers.TestNetwork().AppLoadResponse = null .
        }
    }
}
