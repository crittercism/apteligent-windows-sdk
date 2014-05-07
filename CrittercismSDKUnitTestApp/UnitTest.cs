using CrittercismSDK;
using CrittercismSDK.DataContracts;
using CrittercismSDK.DataContracts.Legacy;
using CrittercismSDK.DataContracts.Unified;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrittercismSDKUnitTestApp {
    [TestClass]
    public class UnitTests
    {
        
        [TestMethod]
        public void TruncatedBreadcrumbTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            TestHelpers.CleanUp(); // drop all previous messages
            // start breadcrumb with sentinel to ensure we don't left-truncate
            string breadcrumb = "raaaaaaaaa";
            for (int x = 0; x < 13; x++)
            {
                breadcrumb += "aaaaaaaaaa";
            }
            // end breadcrumb with "illegal" chars and check for their presence
            breadcrumb += "zzzzzzzzzz";
            Crittercism.LeaveBreadcrumb(breadcrumb);
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                Crittercism.LogHandledException(ex);
            }
            HandledException he = Crittercism.MessageQueue.Dequeue() as HandledException;
            he.DeleteFromDisk();
            Assert.IsNotNull(he, "Expected a HandledException message");
            String asJson = Newtonsoft.Json.JsonConvert.SerializeObject(he);
            Assert.IsTrue(asJson.Contains("\"breadcrumbs\":"));
            Assert.IsTrue(asJson.Contains("\"raaaaaa"));
            Assert.IsFalse(asJson.Contains("aaaaz"));
            Assert.IsFalse(asJson.Contains("zzz"));
        }

        [TestMethod]
        public void LogHandledExceptionTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            Crittercism.LeaveBreadcrumb("HandledExceptionBreadcrumb");
            Crittercism.SetValue("favoriteFood", "Texas Sheet Cake");
            TestHelpers.CleanUp(); // drop all previous messages
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                Crittercism.LogHandledException(ex);
            }
            HandledException he = Crittercism.MessageQueue.Dequeue() as HandledException;
            he.DeleteFromDisk();
            Assert.IsNotNull(he, "Expected a HandledException message");
            String asJson = Newtonsoft.Json.JsonConvert.SerializeObject(he);
            TestHelpers.checkCommonJsonFragments(asJson);
            string[] jsonStrings = new string[] {
                "\"breadcrumbs\":",
                "\"current_session\":[{\"message\":\"HandledExceptionBreadcrumb\"",
                "\"metadata\":{",
                "\"favoriteFood\":\"Texas Sheet Cake\"}",
            };
            foreach (String jsonFragment in jsonStrings)
            {
                Assert.IsTrue(asJson.Contains(jsonFragment));
            }


        }


        

        [TestMethod]
        public void AppLoadCommunicationTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");

            // Create a queuereader
            QueueReader queueReader = new QueueReader();

            // call sendmessage with the appload, no exception should be rise
            queueReader.ReadQueue();
        }

        [TestMethod]
        public void OptOutTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            TestHelpers.CleanUp();
            Assert.IsTrue(Crittercism.MessageQueue == null || Crittercism.MessageQueue.Count == 0);
            Crittercism.SetOptOutStatus(true);
            Assert.IsTrue(Crittercism.CheckOptOutFromDisk());
            Assert.IsTrue(Crittercism.GetOptOutStatus());
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                Crittercism.LogHandledException(ex);
            }
            Assert.IsTrue(Crittercism.MessageQueue == null || Crittercism.MessageQueue.Count == 0);
            // Now turn it back on
            Crittercism.SetOptOutStatus(false);
            Assert.IsFalse(Crittercism.CheckOptOutFromDisk());
            Assert.IsFalse(Crittercism.GetOptOutStatus());
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                Crittercism.LogHandledException(ex);
            }
            Assert.IsTrue(Crittercism.MessageQueue.Count == 1);

        }

        [TestMethod]
        public void HandledExceptionCommunicationTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create the error message
                Crittercism.LogHandledException(ex);

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the error, no exception should be rise
                queueReader.ReadQueue();
            }
        }

        [TestMethod]
        public void CrashCommunicationTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create a Breadcrum
                Crittercism.LeaveBreadcrumb("Breadcrum test");

                // Create the error message
                Crittercism.CreateCrashReport(ex);

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the crash, no exception should be rise
                queueReader.ReadQueue();
            }
        }

        [TestMethod]
        public void AppLoadCommunicationFailTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("WrongAppID");

            // Create a queuereader
            QueueReader queueReader = new QueueReader();

            // call sendmessage with the appload, exception should be rise
            try
            {
                queueReader.ReadQueue();
                Assert.Fail("This ReadQueue method should fail because the AppID is invalid");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(System.Exception), "Expected handled exception with inner message of the webexception");
                Assert.IsInstanceOfType(ex.InnerException, typeof(System.Net.WebException), "Expected handled exception with inner message of the webexception");
            }
        }

        [TestMethod]
        public void HandledExceptionCommunicationFailTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create the error message
                Crittercism.LogHandledException(ex);
                HandledException message = Crittercism.MessageQueue.Last() as HandledException;
                message.app_id = "WrongAppID";

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the error, exception should be rise
                try
                {
                    queueReader.ReadQueue();
                    Assert.Fail("This ReadQueue method should fail because the AppID is invalid");
                }
                catch (Exception e)
                {
                    Assert.IsInstanceOfType(e, typeof(System.Exception), "Expected handled exception with inner message of the webexception");
                    Assert.IsInstanceOfType(e.InnerException, typeof(System.Net.WebException), "Expected handled exception with inner message of the webexception");
                }
            }
        }

        [TestMethod]
        public void CrashCommunicationFailTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create a Breadcrum
                Crittercism.LeaveBreadcrumb("Breadcrum test");

                // Create the crash message
                Crittercism.CreateCrashReport(ex);
                Crash message = Crittercism.MessageQueue.Last() as Crash;
                message.app_id = "WrongAppID";

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the crash, exception should be rise
                try
                {
                    queueReader.ReadQueue();
                    Assert.Fail("This ReadQueue method should fail because the AppID is invalid");
                }
                catch (Exception e)
                {
                    Assert.IsInstanceOfType(e, typeof(System.Exception), "Expected handled exception with inner message of the webexception");
                    Assert.IsInstanceOfType(e.InnerException, typeof(System.Net.WebException), "Expected handled exception with inner message of the webexception");
                }
            }
        }
    }
}
