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
    class HandledExceptionTests {
        [TestMethod]
        public void HandledExceptionDataContractTest() {
            int i = 0;
            int j = 5;
            HandledException newMessageReport = null;
            string errorName = string.Empty;
            string errorMessage = string.Empty;
            string errorStackTrace = string.Empty;
            try {
                int k = j / i;
            } catch (Exception ex) {
                // create new error message
                errorName = ex.GetType().FullName;
                errorMessage = ex.Message;
                errorStackTrace = ex.StackTrace;
                ExceptionObject exception = new ExceptionObject(errorName, errorMessage, errorStackTrace);
                newMessageReport = new HandledException("50807ba33a47481dd5000002", System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString(), new Dictionary<string, string>(), new Breadcrumbs(), exception);
                newMessageReport.SaveToDisk();
            }

            // check that message is saved by try loading it with the helper
            // load saved version of the error event
            HandledException messageReportLoaded = new HandledException();
            messageReportLoaded.Name = newMessageReport.Name;
            messageReportLoaded.LoadFromDisk();

            Assert.IsNotNull(messageReportLoaded);

            // validate that the loaded object is corrected agains the original one via json serialization
            string originalJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport);
            string loadedJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);

            Assert.AreEqual(loadedJsonMessage, originalJsonMessage);

            // compare against known json to verify that the serialization is in the correct format
            TestHelpers.checkCommonJsonFragments(loadedJsonMessage);

            string[] jsonStrings = new string[] {
                "\"error\":{\"name\":\"" + errorName + "\",\"reason\":\"" + errorMessage + "\",\"stack_trace\":[\"" + errorStackTrace.Replace(@"\", @"\\") + "\"]}",
            };
            foreach (string jsonFragment in jsonStrings) {
                Assert.IsTrue(loadedJsonMessage.Contains(jsonFragment));
            }

            // delete the message from disk
            newMessageReport.DeleteFromDisk();
        }
    }
}
