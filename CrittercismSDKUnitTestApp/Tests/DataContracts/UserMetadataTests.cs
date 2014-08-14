using CrittercismSDK;
using CrittercismSDK.DataContracts;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests.DataContracts {
    [TestClass]
    public class UserMetadataTests {
        [TestMethod]
        public void MetadataFormBodyEncodingTest() {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("key with space", "value1");
            dict.Add("key&%", "value2");
            UserMetadata um = new UserMetadata(TestHelpers.VALID_APPID, System.Windows.Application.
                Current.GetType().Assembly.GetName().Version.ToString(), dict);
            string formEncoded = CrittercismSDK.QueueReader.ComputeFormPostBody(um);
            
            // We're form-encoding JSON here...gross
            // Encoded metadata string: {"key with space":"value1","key&%":"value2"}
            // URL encoded form: %7b%22key+with+space%22%3a%22value1%22%2c%22key%26%25%22%3a%22value2%22%7d
            Assert.IsTrue(formEncoded.Contains(
                "%7b%22key+with+space%22%3a%22value1%22%2c%22key%26%25%22%3a%22value2%22%7d"));
        }

        [TestMethod]
        public void AddMetadataTwiceTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("5350bb642bd1f1017c000002");
            Crittercism.SetUsername("george");
            Crittercism.SetValue("username", "ron");
            Crittercism.SetValue("username", "ginny");
            Crittercism.SetUsername("percy");
            Crittercism.SetUsername("charlie");
            Crittercism.SetUsername("bill");
            Crittercism.SetUsername("fred");
            TestHelpers.CleanUp(); // drop all previous messages
            
            try {
                TestHelpers.ThrowDivideByZeroException();
            } catch (Exception ex) {
                Crittercism.CreateCrashReport(ex);
            }

            Crash crash = Crittercism.MessageQueue.Dequeue() as Crash;
            try {
                Assert.IsNotNull(crash, "Expected a Crash message");
            } finally {
                crash.DeleteFromDisk();
            }

            String asJson = Newtonsoft.Json.JsonConvert.SerializeObject(crash);
            Assert.IsTrue(asJson.Contains("fred"));
        }

        [TestMethod]
        public void MetadataPersistenceTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("5350bb642bd1f1017c000002");
            Crittercism.SetUsername("harry");
            Crittercism.SetValue("familiar", "hedwig");
            Assert.AreEqual("hedwig", Crittercism.LoadUserMetadataFromDisk()["familiar"]);
            Assert.AreEqual("harry", Crittercism.LoadUserMetadataFromDisk()["username"]);
            Crittercism.SetUsername("hermione");
            Crittercism.SetValue("familiar", "crookshanks");
            Assert.AreEqual("crookshanks", Crittercism.LoadUserMetadataFromDisk()["familiar"]);
            Assert.AreEqual("hermione", Crittercism.LoadUserMetadataFromDisk()["username"]);
        }
    }
}
