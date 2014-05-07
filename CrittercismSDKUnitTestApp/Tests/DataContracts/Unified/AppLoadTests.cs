using CrittercismSDK.DataContracts.Unified;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests.DataContracts.Unified {
    [TestClass]
    class AppLoadTests {
        [TestMethod]
        public void AppLoadDataContractTest() {
            // create new appload message
            AppLoad newMessageReport = new AppLoad("50807ba33a47481dd5000002");
            newMessageReport.SaveToDisk();

            // check that message is saved by try loading it with the helper
            // load saved version of the appload event
            AppLoad messageReportLoaded = new AppLoad();
            messageReportLoaded.Name = newMessageReport.Name;
            messageReportLoaded.LoadFromDisk();

            Assert.IsNotNull(messageReportLoaded);

            // validate that the loaded object is corrected agains the original one via json serialization
            string originalJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport);
            string loadedJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);

            Assert.AreEqual(loadedJsonMessage, originalJsonMessage);

            // compare against known json to verify that the serialization is in the correct format
            TestHelpers.checkCommonJsonFragments(loadedJsonMessage);

            // delete the message from disk
            newMessageReport.DeleteFromDisk();
        }
    }
}
