using CrittercismSDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
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
    public class LogNetworkRequestTests {
        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            Crittercism.Shutdown();
            TestHelpers.Cleanup();
        }
        [TestMethod]
        public void LogNetworkRequestTest() {
            APM.interval = 1;
            TestHelpers.StartApp();
            Assert.IsNull(TestHelpers.TestNetwork().AppLoadResponse);
            Assert.IsTrue(APM.enabled);
            string method = "GET";
            string uriString = "http://www.mrscritter.com";
            long latency = 4000;
            long bytesRead = 10000;
            long bytesSent = 10000;
            long responseCode = 200;
            Crittercism.LogNetworkRequest(
                method,
                uriString,
                latency,
                bytesRead,
                bytesSent,
                (HttpStatusCode)responseCode,
                WebExceptionStatus.Success);
            Trace.WriteLine("Crittercism.LogNetworkRequest returned");
            Trace.WriteLine("APM.enabled == " + APM.enabled);
            MessageReport messageReport = TestHelpers.DequeueMessageType(typeof(APMReport));
            if (messageReport != null) {
                Trace.WriteLine("We found an APMReport (YAY)");
            } else {
                Trace.WriteLine("We didn't find an APMReport (BOO)");
                Trace.WriteLine("APM.enabled == " + APM.enabled);
                Assert.IsNotNull(messageReport,"Expected an APMReport message");
            };
            String asJson = JsonConvert.SerializeObject(messageReport);
            Trace.WriteLine("asJson == " + asJson);
            string[] jsonStrings = new string[] {
                    "\"d\":",
                    "\"GET\"",
                    "\"http://www.mrscritter.com\"",
                    "4000",
                    "10000",
                    "200"
                };
            foreach (String jsonFragment in jsonStrings) {
                Trace.WriteLine("jsonFragment == " + jsonFragment);
                Trace.WriteLine("asJson.Contains(jsonFragment) == " + asJson.Contains(jsonFragment));
                Assert.IsTrue(asJson.Contains(jsonFragment));
            };
        }
    }
}
