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
        [TestMethod]
        public void LogNetworkRequestTest() {
            APM.NETWORK_SEND_INTERVAL=1;
            TestHelpers.StartApp();
            string method="GET";
            string uriString="http://www.mrscritter.com";
            long latency=4000;
            long bytesRead=10000;
            long bytesSent=10000;
            long responseCode=200;
            Crittercism.LogNetworkRequest(
                method,
                uriString,
                latency,
                bytesRead,
                bytesSent,
                (HttpStatusCode)responseCode,
                WebExceptionStatus.Success);
            MessageReport messageReport=null;
            for (int i=1; i<=10; i++) {
                Thread.Sleep(100);
                messageReport=TestHelpers.DequeueMessageType(typeof(APMReport));
                if (messageReport!=null) {
                    break;
                }
            }
            Assert.IsNotNull(messageReport,"Expected an APMReport message");
            String asJson=JsonConvert.SerializeObject(messageReport);
            Debug.WriteLine("asJson == "+asJson);
            string[] jsonStrings=new string[] {
                "\"d\":",
                "\"GET\"",
                "\"http://www.mrscritter.com\"",
                "4000",
                "10000",
                "200"
            };
            foreach (String jsonFragment in jsonStrings) {
                Debug.WriteLine("jsonFragment == "+jsonFragment);
                Debug.WriteLine("asJson.Contains(jsonFragment) == "+asJson.Contains(jsonFragment));
                Assert.IsTrue(asJson.Contains(jsonFragment));
            };
        }
    }
}
