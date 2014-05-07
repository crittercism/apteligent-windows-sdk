using CrittercismSDK;
using CrittercismSDK.DataContracts.Legacy;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests.DataContracts.Legacy {
    [TestClass]
    class UserMetadataTests {
        [TestMethod]
        public void ComputeMetadataFormPostBodyTest() {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("key with space", "value1");
            dict.Add("key&%", "value2");
            UserMetadata um = new UserMetadata("50807ba33a47481dd5000002", System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString(), dict);
            string formEncoded = CrittercismSDK.QueueReader.ComputeFormPostBody(um);
            Assert.IsTrue(formEncoded.Contains("%26")); // ampersand
            Assert.IsTrue(formEncoded.Contains("%25")); // percent
            Assert.IsFalse(formEncoded.Contains("{"));
            Assert.IsFalse(formEncoded.Contains("\""));
        }

        [TestMethod]
        public void AddMetadataTwiceTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            Crittercism.SetUsername("george");
            Crittercism.SetValue("username", "ron");
            Crittercism.SetValue("username", "ginny");
            Crittercism.SetUsername("percy");
            Crittercism.SetUsername("charlie");
            Crittercism.SetUsername("bill");
            Crittercism.SetUsername("fred");
            TestHelpers.CleanUp(); // drop all previous messages
            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                Crittercism.CreateCrashReport(ex);
            }
            Crash crash = Crittercism.MessageQueue.Dequeue() as Crash;
            crash.DeleteFromDisk();
            Assert.IsNotNull(crash, "Expected a Crash message");
            String asJson = Newtonsoft.Json.JsonConvert.SerializeObject(crash);
            Assert.IsTrue(asJson.Contains("fred"));
        }

        [TestMethod]
        public void MetadataPersistenceTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
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
