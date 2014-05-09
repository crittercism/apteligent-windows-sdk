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
    public class HandledExceptionTests {
        [TestMethod]
        public void HandledExceptionDataContractTest() {
            HandledException newMessageReport = null;
            try {
                string errorName = string.Empty;
                string errorMessage = string.Empty;
                string errorStackTrace = string.Empty;
                try {
                    TestHelpers.ThrowDivideByZeroException();
                } catch (Exception ex) {
                    // create new error message
                    errorName = ex.GetType().FullName;
                    errorMessage = ex.Message;
                    errorStackTrace = ex.StackTrace;
                    ExceptionObject exception = new ExceptionObject(errorName, errorMessage, errorStackTrace);
                    newMessageReport = new HandledException(TestHelpers.VALID_APPID,
                        System.Windows.Application.Current.GetType().Assembly.GetName().Version.
                        ToString(), new Dictionary<string, string>(), new Breadcrumbs(),
                        exception);
                    newMessageReport.SaveToDisk();
                }

                HandledException messageReportLoaded = new HandledException();
                messageReportLoaded.Name = newMessageReport.Name;
                messageReportLoaded.LoadFromDisk();

                Assert.IsNotNull(messageReportLoaded);

                // validate that the loaded object is corrected agains the original one via json serialization
                string originalJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport);
                string loadedJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);

                Assert.AreEqual(loadedJsonMessage, originalJsonMessage);

                // compare against known json to verify that the serialization is in the correct format
                TestHelpers.CheckCommonJsonFragments(loadedJsonMessage);

                string[] jsonStrings = new string[] {
                    "\"error\":{\"name\":\"" + errorName + "\",\"reason\":\"" + errorMessage + "\",\"stack_trace\":[\"" + errorStackTrace.Replace(@"\", @"\\") + "\"]}",
                };
                foreach (string jsonFragment in jsonStrings) {
                    Assert.IsTrue(loadedJsonMessage.Contains(jsonFragment));
                }
            } finally {
                newMessageReport.DeleteFromDisk();
            }
        }

        [TestMethod]
        public void HandledExceptionCommunicationTest() {
            TestHelpers.InitializeRemoveLoadFromQueue(TestHelpers.VALID_APPID);
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            
            try {
                TestHelpers.ThrowDivideByZeroException();
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
                QueueReader queueReader = new QueueReader();
                queueReader.ReadQueue();
            }
        }

        [TestMethod]
        public void HandledExceptionCommunicationFailTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            
            try {
                TestHelpers.ThrowDivideByZeroException();
            } catch (Exception ex) {
                // Create the error message
                Crittercism.LogHandledException(ex);
                HandledException message = Crittercism.MessageQueue.Last() as HandledException;
                message.app_id = "WrongAppID";

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the error, exception should be rise
                Assert.ThrowsException<System.Exception>(() => queueReader.ReadQueue());
                /*
                try {
                    
                    Assert.Fail("This ReadQueue method should fail because the AppID is invalid");
                } catch (Exception e) {
                    Assert.IsInstanceOfType(e, typeof(System.Exception), "Expected handled exception with inner message of the webexception");
                    Assert.IsInstanceOfType(e.InnerException, typeof(System.Net.WebException), "Expected handled exception with inner message of the webexception");
                }
                */
            }
        }
    }
}
