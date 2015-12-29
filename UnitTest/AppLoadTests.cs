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
    public class AppLoadTests : ITest {
        private volatile bool AppLoadResponseReceived = false;
        private string ExampleAppLoadResponse = (
            "{\"txnConfig\":{\"defaultTimeout\":3600000,\n"
            + "              \"interval\":10,\n"
            + "              \"enabled\":true,\n"
            + "              \"transactions\":{\"Buy Critter Feed\":{\"timeout\":60000,\"slowness\":3600000,\"value\":1299},\n"
            + "                              \"Sing Critter Song\":{\"timeout\":90000,\"slowness\":3600000,\"value\":1500},\n"
            + "                              \"Write Critter Poem\":{\"timeout\":60000,\"slowness\":3600000,\"value\":2000}}},\n"
            + " \"apm\":{\"net\":{\"enabled\":true,\n"
            + "               \"persist\":false,\n"
            + "               \"interval\":10}},\n"
            + " \"needPkg\":1,\n"
            + " \"internalExceptionReporting\":true}"
        );
        public bool SendRequest(MessageReport message) {
            message.DidReceiveResponse(ExampleAppLoadResponse);
            if (message is AppLoad) {
                AppLoadResponseReceived = true;
            }
            return true;
        }
        [TestInitialize()]
        public void TestInitialize() {
            AppLoadResponseReceived = false;
        }
        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            Crittercism.Shutdown();
            TestHelpers.Cleanup();
        }
        [TestMethod]
        public void AppLoadTest1() {
            // Test AppLoad json message that is sent.
            {
                // Crittercism.Test == null means no messageReport's get sent, but stay in queue.
                Assert.IsNull(Crittercism.Test);
            }
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
                // Crittercism.Test == this means messageReport's received by test.
                Crittercism.Test = this;
            }
            // StartApp
            TestHelpers.StartApp();
            // Wait sufficiently long for AppLoad response.
            for (int i = 1; i < 10; i++) {
                Thread.Sleep(100);
                if (AppLoadResponseReceived) {
                    break;
                }
            }
            Assert.IsTrue(AppLoadResponseReceived);
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
