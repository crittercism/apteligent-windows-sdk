using CrittercismSDK;
using CrittercismSDK.DataContracts.Unified;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests.DataContracts.Unified {
    [TestClass]
    public class AppLoadTests {
        [TestMethod]
        public void AppLoadDiskRoundtrip() {
            // There is so much wrong here I don't know where to begin...
            // Refactor this byzantine load/store thing to use a platform service
            // Use more self-describing persistence strategy for messages, to avoid need to set
            //   "Name" before loading
            // Also, concerns are inappropriately mixed, should CREATE then SAVE object, not do
            //   this as a single bundled/atomic action

            AppLoad newMessageReport = new AppLoad(TestHelpers.VALID_APPID);

            try {    
                newMessageReport.SaveToDisk();

                AppLoad loadedMessageReport = new AppLoad {
                    Name = newMessageReport.Name
                };

                loadedMessageReport.LoadFromDisk();
            
                // TODO(DA) should do overlaoded object equality test here
                // We can't right now because types get mangled when round-tripping to/from JSON :/
                //Assert.IsTrue(newMessageReport.Equals(loadedMessageReport));

                Assert.AreEqual(Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport),
                    Newtonsoft.Json.JsonConvert.SerializeObject(loadedMessageReport));
            } finally {
                newMessageReport.DeleteFromDisk();
            }
        }

        [TestMethod]
        public void AppLoadFormat() {
            AppLoad newMessageReport = new AppLoad(TestHelpers.VALID_APPID);
            AppLoadInner inner = newMessageReport.appLoads;

            Assert.AreEqual(newMessageReport.count, 1);
            Assert.AreEqual(newMessageReport.current, true);
            Assert.AreEqual(inner.osName, "wp");
            Assert.AreEqual(inner.carrier, "Fake GSM Network");     // On emulator
        }

        [TestMethod]
        public void AppLoadCommunicationTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");

            // Create a queuereader
            QueueReader queueReader = new QueueReader();

            // call sendmessage with the appload, no exception should be rise
            queueReader.ReadQueue();
        }
    }
}
