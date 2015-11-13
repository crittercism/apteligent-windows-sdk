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
    public class MetadataReportTests {
        [TestMethod]
        public void MetadataEncodingTest() {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("key with space", "value1");
            dict.Add("key&%", "value2");
            MetadataReport metadataReport = new MetadataReport(TestHelpers.VALID_APPID, dict);
            string formEncoded = CrittercismSDK.QueueReader.ComputeFormPostBody(metadataReport);
            // We're form-encoding JSON here...gross
            // Encoded metadata string: {"key with space":"value1","key&%":"value2"}
            // URL encoded form: %7b%22key+with+space%22%3a%22value1%22%2c%22key%26%25%22%3a%22value2%22%7d
            Assert.IsTrue(formEncoded.Contains(
                "%7b%22key+with+space%22%3a%22value1%22%2c%22key%26%25%22%3a%22value2%22%7d"));
        }

        [TestMethod]
        public void AddMetadataTwiceTest() {
            try {
                TestHelpers.StartApp();
                Crittercism.SetUsername("hamster");
                Crittercism.SetUsername("robin");
                Crittercism.SetUsername("squirrel");
                TestHelpers.LogHandledException();
                MessageReport messageReport = TestHelpers.DequeueMessageType(typeof(HandledException));
                String asJson = JsonConvert.SerializeObject(messageReport);
                Assert.IsFalse(asJson.Contains("hamster"));
                Assert.IsFalse(asJson.Contains("robin"));
                Assert.IsTrue(asJson.Contains("squirrel"));
            } finally {
                Crittercism.Shutdown();
                TestHelpers.Cleanup();
            }
        }

        [TestMethod]
        public void MetadataPersistenceTest() {
            Crittercism.autoRunQueueReader = false;
            Crittercism.Init("5350bb642bd1f1017c000002");
            Crittercism.SetUsername("harry");
            Crittercism.SetValue("surname", "hedwig");
            Debug.WriteLine("surname == "+Crittercism.ValueFor("surname"));
            Assert.AreEqual(Crittercism.ValueFor("surname"),"hedwig");
            Assert.AreEqual(Crittercism.ValueFor("username"),"harry");
            Crittercism.SetUsername("hermione");
            Crittercism.SetValue("surname","crookshanks");
            Assert.AreEqual(Crittercism.ValueFor("surname"),"crookshanks");
            Assert.AreEqual(Crittercism.ValueFor("username"),"hermione");
        }
    }
}
