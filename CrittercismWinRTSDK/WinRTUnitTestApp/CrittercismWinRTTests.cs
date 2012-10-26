namespace WinRTUnitTestApp
{
    using CrittercismSDK;
    using CrittercismSDK.DataContracts;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;

    [TestClass]
    public class CrittercismWinRTTests
    {
        [TestMethod, TestCategory("DataContract Tests")]
        public void AppLoadDataContractTest()
        {
            // create new appload message
            AppLoad newMessageReport = new AppLoad("50807ba33a47481dd5000002");
            newMessageReport.SaveToDisk();

            // check that message is saved by try loading it with the helper
            // load saved version of the appload event
            AppLoad messageReportLoaded = null;
            messageReportLoaded = (AppLoad)StorageHelper.LoadFromDisk(typeof(AppLoad), Crittercism.messageFolder, newMessageReport.Name);
            
            Assert.IsNotNull(messageReportLoaded);

            // validate that the loaded object is corrected agains the original one via json serialization
            string originalJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport);
            string loadedJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);
            
            Assert.AreEqual(loadedJsonMessage, originalJsonMessage);
            
            // compare against known json to verify that the serialization is in the correct format
            string jsonString = "{\"app_id\":\"50807ba33a47481dd5000002\",\"app_state\":{\"app_version\":\"" + Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0] + "\",\"battery_level\":\"50\"},\"platform\":{\"client\":\"winRTv1.0\",\"device_id\":\"" + Crittercism.DeviceId + "\",\"device_model\":\"Nokia Lumia 800\",\"os_name\":\"Windows Phone\",\"os_version\":\"8.0\"}}";

            Assert.AreEqual(loadedJsonMessage, jsonString);

            // delete the message from disk
            newMessageReport.DeleteFromDisk();
        }

        [TestMethod, TestCategory("DataContract Tests")]
        public void ErrorDataContractTest()
        {
            int i = 0;
            int j = 5;
            Error newMessageReport = null;
            string errorName = string.Empty;
            string errorMessage = string.Empty;
            string errorStackTrace = string.Empty;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // create new error message
                errorName = ex.GetType().FullName;
                errorMessage = ex.Message;
                errorStackTrace = ex.StackTrace;
                ExceptionObject exception = new ExceptionObject(errorName, errorMessage, errorStackTrace);
                newMessageReport = new Error("50807ba33a47481dd5000002", exception);
                newMessageReport.SaveToDisk();
            }
            
            // check that message is saved by try loading it with the helper
            // load saved version of the error event
            Error messageReportLoaded = null;
            messageReportLoaded = (Error)StorageHelper.LoadFromDisk(typeof(Error), Crittercism.messageFolder, newMessageReport.Name);

            Assert.IsNotNull(messageReportLoaded);

            // validate that the loaded object is corrected agains the original one via json serialization
            string originalJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport);
            string loadedJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);

            Assert.AreEqual(loadedJsonMessage, originalJsonMessage);

            // compare against known json to verify that the serialization is in the correct format
            string jsonString = "{\"app_id\":\"50807ba33a47481dd5000002\",\"app_state\":{\"app_version\":\"" + Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0] + "\",\"battery_level\":\"50\"},\"error\":{\"name\":\"" + errorName + "\",\"reason\":\"" + errorMessage + "\",\"stack_trace\":[\"" + errorStackTrace.Replace(@"\", @"\\") + "\"]},\"platform\":{\"client\":\"winRTv1.0\",\"device_id\":\"" + Crittercism.DeviceId + "\",\"device_model\":\"Nokia Lumia 800\",\"os_name\":\"Windows Phone\",\"os_version\":\"8.0\"}}";

            Assert.AreEqual(loadedJsonMessage, jsonString);

            // delete the message from disk
            newMessageReport.DeleteFromDisk();
        }

        [TestMethod, TestCategory("DataContract Tests")]
        public void CrashDataContractTest()
        {
            int i = 0;
            int j = 5;
            Crash newMessageReport = null;
            string errorName = string.Empty;
            string errorMessage = string.Empty;
            string errorStackTrace = string.Empty;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // create new crash message
                errorName = ex.GetType().FullName;
                errorMessage = ex.Message;
                errorStackTrace = ex.StackTrace;
                ExceptionObject exception = new ExceptionObject(errorName, errorMessage, errorStackTrace);
                newMessageReport = new Crash("50807ba33a47481dd5000002", new Breadcrumbs(), exception);
                newMessageReport.SaveToDisk();
            }

            // check that message is saved by try loading it with the helper
            // load saved version of the crash event
            Crash messageReportLoaded = null;
            messageReportLoaded = (Crash)StorageHelper.LoadFromDisk(typeof(Crash), Crittercism.messageFolder, newMessageReport.Name);

            Assert.IsNotNull(messageReportLoaded);

            // validate that the loaded object is corrected agains the original one via json serialization
            string originalJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(newMessageReport);
            string loadedJsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageReportLoaded);

            Assert.AreEqual(loadedJsonMessage, originalJsonMessage);

            // compare against known json to verify that the serialization is in the correct format
            string jsonString = "{\"app_id\":\"50807ba33a47481dd5000002\",\"app_state\":{\"app_version\":\"" + Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0] + "\",\"battery_level\":\"50\"},\"breadcrumbs\":{\"current_session\":[],\"previous_session\":[]},\"crash\":{\"name\":\"" + errorName + "\",\"reason\":\"" + errorMessage + "\",\"stack_trace\":[\"" + errorStackTrace.Replace(@"\", @"\\") + "\"]},\"platform\":{\"client\":\"winRTv1.0\",\"device_id\":\"" + Crittercism.DeviceId + "\",\"device_model\":\"Nokia Lumia 800\",\"os_name\":\"Windows Phone\",\"os_version\":\"8.0\"}}";

            Assert.AreEqual(loadedJsonMessage, jsonString);

            // delete the message from disk
            newMessageReport.DeleteFromDisk();
        }

        [TestMethod, TestCategory("QueueManagement Tests")]
        public void AppLoadQueueManagementTest()
        {
            // Disable the auto run functionality for the queue message, to verify that the appload message is enqueue correctly
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            // The init method creates an appLoad message
            if (Crittercism.MessageQueue.Count == 1)
            {
                AppLoad appLoad = Crittercism.MessageQueue.Dequeue() as AppLoad;
                // verify that the message in the queue is an appLoad type
                Assert.IsNotNull(appLoad, "The message isn´t AppLoad type");

                // verify each field of the appLoad message
                Assert.AreEqual("50807ba33a47481dd5000002", appLoad.app_id, "The app_id is incorrect");
                Assert.AreEqual(Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0], appLoad.app_state["app_version"], "The app_version is incorrect");
                Assert.AreEqual("50", appLoad.app_state["battery_level"], "The battery_level is incorrect");
                Assert.AreEqual("winRTv1.0", appLoad.platform.client, "The client is incorrect");
                Assert.AreEqual(Crittercism.DeviceId, appLoad.platform.device_id, "The device_id is incorrect");
                Assert.AreEqual("Nokia Lumia 800", appLoad.platform.device_model, "The device_model is incorrect");
                Assert.AreEqual("Windows Phone", appLoad.platform.os_name, "The os_name is incorrect");
                Assert.AreEqual("8.0", appLoad.platform.os_version, "The os_version is incorrect");
                appLoad.DeleteFromDisk();
            }
            else
            {
                Assert.Fail("The AppLoad message isn't in the queue");
            }
        }

        [TestMethod, TestCategory("QueueManagement Tests")]
        public void ErrorQueueManagementTest()
        {
            // Disable the auto run functionality for the queue message, to verify that the error message is enqueue correctly
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            // Eliminating the appLoad message enqueued because isn't necessary the verification of that
            MessageReport message = Crittercism.MessageQueue.Dequeue();
            message.DeleteFromDisk();

            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create the error message
                Crittercism.CreateErrorReport(ex);
                if (Crittercism.MessageQueue.Count == 1)
                {
                    Error error = Crittercism.MessageQueue.Dequeue() as Error;
                    // verify that the message in the queue is an error type
                    Assert.IsNotNull(error, "The message isn´t Error type");

                    // verify each field of the error message
                    Assert.AreEqual("50807ba33a47481dd5000002", error.app_id, "The app_id is incorrect");
                    Assert.AreEqual(Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0], error.app_state["app_version"], "The app_version is incorrect");
                    Assert.AreEqual("50", error.app_state["battery_level"], "The battery_level is incorrect");
                    Assert.AreEqual(ex.GetType().FullName, error.error.name, "The error name is incorrect");
                    Assert.AreEqual(ex.Message, error.error.reason, "The error reason is incorrect");
                    List<string> stackTraceList = ex.StackTrace.Split(new string[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (int index = 0; index < stackTraceList.Count; index++)
			        {
                        Assert.AreEqual(stackTraceList[index], error.error.stack_trace[index], "The stack_trace is incorrect");
			        }

                    Assert.AreEqual("winRTv1.0", error.platform.client, "The client is incorrect");
                    Assert.AreEqual(Crittercism.DeviceId, error.platform.device_id, "The device_id is incorrect");
                    Assert.AreEqual("Nokia Lumia 800", error.platform.device_model, "The device_model is incorrect");
                    Assert.AreEqual("Windows Phone", error.platform.os_name, "The os_name is incorrect");
                    Assert.AreEqual("8.0", error.platform.os_version, "The os_version is incorrect");
                    error.DeleteFromDisk();
                }
                else
                {
                    Assert.Fail("The Error message isn't in the queue");
                }
            }
        }

        [TestMethod, TestCategory("QueueManagement Tests")]
        public void CrashQueueManagementTest()
        {
            // Disable the auto run functionality for the queue message, to verify that the crash message is enqueue correctly
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            // Eliminating the appLoad message enqueued because isn't necessary the verification of that
            MessageReport message = Crittercism.MessageQueue.Dequeue();
            message.DeleteFromDisk();

            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create a Breadcrumb to verify this in the message
                Crittercism.LeaveBreadcrum("Breadcrumb test");

                // Create the crash message
                Crittercism.CreateCrashReport(ex, ex.StackTrace);
                if (Crittercism.MessageQueue.Count == 1)
                {
                    Crash crash = Crittercism.MessageQueue.Dequeue() as Crash;
                    // verify that the message in the queue is a crash type
                    Assert.IsNotNull(crash, "The message isn´t Crash type");

                    // verify each field of the crash message
                    Assert.AreEqual("50807ba33a47481dd5000002", crash.app_id, "The app_id is incorrect");
                    Assert.AreEqual(Application.Current.GetType().AssemblyQualifiedName.Split('=')[1].Split(',')[0], crash.app_state["app_version"], "The app_version is incorrect");
                    Assert.AreEqual("50", crash.app_state["battery_level"], "The battery_level is incorrect");
                    Assert.AreEqual("Breadcrumb test", crash.breadcrumbs.current_session[0].message, "The breadcrumb message is incorrect");
                    Assert.AreEqual(ex.GetType().FullName, crash.crash.name, "The crash name is incorrect");
                    Assert.AreEqual(ex.Message, crash.crash.reason, "The crash reason is incorrect");
                    List<string> stackTraceList = ex.StackTrace.Split(new string[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (int index = 0; index < stackTraceList.Count; index++)
                    {
                        Assert.AreEqual(stackTraceList[index], crash.crash.stack_trace[index], "The stack_trace is incorrect");
                    }

                    Assert.AreEqual("winRTv1.0", crash.platform.client, "The client is incorrect");
                    Assert.AreEqual(Crittercism.DeviceId, crash.platform.device_id, "The device_id is incorrect");
                    Assert.AreEqual("Nokia Lumia 800", crash.platform.device_model, "The device_model is incorrect");
                    Assert.AreEqual("Windows Phone", crash.platform.os_name, "The os_name is incorrect");
                    Assert.AreEqual("8.0", crash.platform.os_version, "The os_version is incorrect");
                    crash.DeleteFromDisk();
                }
                else
                {
                    Assert.Fail("The Crash message isn't in the queue");
                }
            }
        }

        [TestMethod, TestCategory("QueueManagement Tests")]
        public void ThreadQueueManagementTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableCommunicationLayer = false;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            Crittercism.LeaveBreadcrum("Breadcrumb test");
            
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                Crittercism.CreateErrorReport(ex);
                Crittercism.CreateCrashReport(ex, ex.StackTrace);
                if (Crittercism.MessageQueue.Count == 3)
                {
                    // start manually the thread and ensure that the messages are consume
                    lock (Crittercism.readerTask)
                    {
                        if (Crittercism.readerTask.Status == TaskStatus.Created)
                        {
                            Crittercism.readerTask.Start();
                        }
                        else if (Crittercism.readerTask.Status == TaskStatus.RanToCompletion || Crittercism.readerTask.Status == TaskStatus.Faulted)
                        {
                            QueueReader queueReader = new QueueReader();
                            Crittercism.readerTask = new Task(queueReader.ReadQueue);
                            Crittercism.readerTask.Start();
                        }
                    }

                    // Waiting for finish the task
                    Crittercism.readerTask.Wait();
                    if (Crittercism.MessageQueue.Count != 0)
                    {
                        Assert.Fail("All the messages aren't consume correctly");
                    }
                }
                else
                {
                    Assert.Fail("The messages aren't in the queue");
                }
            }
        }

        [TestMethod, TestCategory("Communication Tests")]
        public void AppLoadCommnunicationTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");

            // Create a queuereader
            QueueReader queueReader = new QueueReader();
            
            // call sendmessage with the appload, no exception should be rise
            queueReader.ReadQueue();
        }

        [TestMethod, TestCategory("Communication Tests")]
        public void ErrorCommnunicationTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create the error message
                Crittercism.CreateErrorReport(ex);

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the error, no exception should be rise
                queueReader.ReadQueue();
            }
        }

        [TestMethod, TestCategory("Communication Tests")]
        public void CrashCommnunicationTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create a Breadcrum
                Crittercism.LeaveBreadcrum("Breadcrum test");

                // Create the error message
                Crittercism.CreateCrashReport(ex, ex.StackTrace);

                // Create a queuereader
                QueueReader queueReader = new QueueReader();

                // call sendmessage with the crash, no exception should be rise
                queueReader.ReadQueue();
            }
        }

        [TestMethod, TestCategory("Communication Tests")]
        public void AppLoadCommnunicationFailTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("WrongAppID", "key", "secret");

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

        [TestMethod, TestCategory("Communication Tests")]
        public void ErrorCommnunicationFailTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create the error message
                Crittercism.CreateErrorReport(ex);
                Error message = Crittercism.MessageQueue.Last() as Error;
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

        [TestMethod, TestCategory("Communication Tests")]
        public void CrashCommnunicationFailTest()
        {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableRaiseExceptionInCommunicationLayer = true;
            Crittercism.Init("50807ba33a47481dd5000002", "key", "secret");
            int i = 0;
            int j = 5;
            try
            {
                int k = j / i;
            }
            catch (Exception ex)
            {
                // Create a Breadcrum
                Crittercism.LeaveBreadcrum("Breadcrum test");

                // Create the crash message
                Crittercism.CreateCrashReport(ex, ex.StackTrace);
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

        [TestCleanup]
        public void CleanUp()
        {
            // This method is for clean all the possible variables that may be will used by another unit test
            Crittercism._autoRunQueueReader = true;
            Crittercism._enableCommunicationLayer = true;
            Crittercism._enableRaiseExceptionInCommunicationLayer = false;
            while (Crittercism.MessageQueue != null && Crittercism.MessageQueue.Count > 0)
            {
                MessageReport message = Crittercism.MessageQueue.Dequeue();
                message.DeleteFromDisk();
            }
        }
    }
}
