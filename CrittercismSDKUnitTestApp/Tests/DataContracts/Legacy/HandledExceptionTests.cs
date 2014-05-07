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

        [TestMethod]
        public void HandledExceptionCommunicationTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                // Create the error message
                Crittercism.LogHandledException(ex);

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the error, no exception should be rise
                queueReader.ReadQueue();
            }
        }

        [TestMethod]
        public void HandledExceptionCommunicationFailTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                // Create the error message
                Crittercism.LogHandledException(ex);
                HandledException message = Crittercism.MessageQueue.Last() as HandledException;
                message.app_id = "WrongAppID";

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the error, exception should be rise
                try {
                    queueReader.ReadQueue();
                    Assert.Fail("This ReadQueue method should fail because the AppID is invalid");
                } catch (Exception e) {
                    Assert.IsInstanceOfType(e, typeof(System.Exception), "Expected handled exception with inner message of the webexception");
                    Assert.IsInstanceOfType(e.InnerException, typeof(System.Net.WebException), "Expected handled exception with inner message of the webexception");
                }
            }
        }
    }
}
