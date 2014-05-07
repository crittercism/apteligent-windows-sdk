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
            // Also, shouldn't require random fields to be set arbitrarily for loads?!
            // Also, concerns are inappropriately mixed, should CREATE then SAVE object, not do
            //   this as a single bundled/atomic action

            AppLoad newMessageReport = new AppLoad(TestHelpers.VALID_APPID);
            newMessageReport.SaveToDisk();

            AppLoad loadedMessageReport = new AppLoad();
            loadedMessageReport.Name = newMessageReport.Name;
            loadedMessageReport.LoadFromDisk();

            Assert.IsTrue(newMessageReport.Equals(loadedMessageReport));
            newMessageReport.DeleteFromDisk();
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
