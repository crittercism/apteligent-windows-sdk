using CrittercismSDK;
using CrittercismSDK.DataContracts;
using CrittercismSDK.DataContracts.Legacy;
using CrittercismSDK.DataContracts.Unified;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests {
    [TestClass]
    class QueueManagementTests {
        [TestMethod]
        public void AppLoadQueueManagementTest()
        {
            // Disable the auto run functionality for the queue message, to verify that the appload message is enqueue correctly
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            // The init method creates an appLoad message
            if (Crittercism.MessageQueue.Count == 1)
            {
                AppLoad appLoad = Crittercism.MessageQueue.Dequeue() as AppLoad;
                // verify that the message in the queue is an appLoad type
                Assert.IsNotNull(appLoad, "The message isn´t AppLoad type");

                // verify each field of the appLoad message
                Platform p = new Platform();
                Assert.AreEqual("50807ba33a47481dd5000002", appLoad.appLoads.appID, "The app_id is incorrect");
                appLoad.DeleteFromDisk();
            }
            else
            {
                Assert.Fail("The AppLoad message isn't in the queue");
            }
        }

        [TestMethod]
        public void HandledExceptionQueueManagementTest() {
            // Disable the auto run functionality for the queue message, to verify that the error message is enqueue correctly
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            // Eliminating the appLoad message enqueued because isn't necessary the verification of that
            MessageReport message = Crittercism.MessageQueue.Dequeue();
            message.DeleteFromDisk();

            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                // Create the error message
                Crittercism.LogHandledException(ex);
                if (Crittercism.MessageQueue.Count == 1) {
                    HandledException he = Crittercism.MessageQueue.Dequeue() as HandledException;
                    // verify that the message in the queue is a HandledException type
                    Assert.IsNotNull(he, "The message isn't HandledException type");

                    // verify each field of the error message
                    Assert.AreEqual("50807ba33a47481dd5000002", he.app_id, "The app_id is incorrect");
                    Assert.AreEqual(System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString(), he.app_state["app_version"], "The app_version is incorrect");
                    //Assert.AreEqual(Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent.ToString(), error.app_state["battery_level"], "The battery_level is incorrect");
                    Assert.AreEqual(ex.GetType().FullName, he.error.name, "The error name is incorrect");
                    Assert.AreEqual(ex.Message, he.error.reason, "The error reason is incorrect");
                    List<string> stackTraceList = ex.StackTrace.Split(new string[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (int index = 0; index < stackTraceList.Count; index++) {
                        Assert.AreEqual(stackTraceList[index], he.error.stack_trace[index], "The stack_trace is incorrect");
                    }

                    Platform p = new Platform();
                    //Assert.AreEqual("wp8v1.0", error.platform.client, "The client is incorrect");
                    Assert.AreEqual(p.device_id, he.platform.device_id, "The device_id is incorrect");
                    //Assert.AreEqual("Nokia Lumia 800", error.platform.device_model, "The device_model is incorrect");
                    //Assert.AreEqual("Windows Phone", error.platform.os_name, "The os_name is incorrect");
                    //Assert.AreEqual("8.0", error.platform.os_version, "The os_version is incorrect");
                    he.DeleteFromDisk();
                } else {
                    Assert.Fail("HandledException message isn't in the queue");
                }
            }
        }

        [TestMethod]
        public void CrashQueueManagementTest() {
            Console.WriteLine("Starting CrashQueueManagementTest");
            // Disable the auto run functionality for the queue message, to verify that the crash message is enqueue correctly
            Crittercism._autoRunQueueReader = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            // Eliminating the appLoad message enqueued because isn't necessary the verification of that
            MessageReport message = Crittercism.MessageQueue.Dequeue();
            message.DeleteFromDisk();

            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                // Create a Breadcrumb to verify this in the message
                Crittercism.LeaveBreadcrumb("Breadcrumb test");

                // Create the crash message
                Crittercism.CreateCrashReport(ex);
                if (Crittercism.MessageQueue.Count == 1) {
                    Crash crash = Crittercism.MessageQueue.Dequeue() as Crash;
                    // verify that the message in the queue is a crash type
                    Assert.IsNotNull(crash, "The message isn´t Crash type");

                    // verify each field of the crash message
                    Assert.AreEqual("50807ba33a47481dd5000002", crash.app_id, "The app_id is incorrect");
                    Assert.AreEqual(System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString(), crash.app_state["app_version"], "The app_version is incorrect");
                    //Assert.AreEqual(Windows.Phone.Devices.Power.Battery.GetDefault().RemainingChargePercent.ToString(), crash.app_state["battery_level"], "The battery_level is incorrect");
                    Assert.AreEqual("Breadcrumb test", crash.breadcrumbs.current_session[0].message, "The breadcrumb message is incorrect");
                    Assert.AreEqual(ex.GetType().FullName, crash.crash.name, "The crash name is incorrect");
                    Assert.AreEqual(ex.Message, crash.crash.reason, "The crash reason is incorrect");
                    List<string> stackTraceList = ex.StackTrace.Split(new string[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (int index = 0; index < stackTraceList.Count; index++) {
                        Assert.AreEqual(stackTraceList[index], crash.crash.stack_trace[index], "The stack_trace is incorrect");
                    }

                    Platform p = new Platform();
                    //Assert.AreEqual("wp8v1.0", crash.platform.client, "The client is incorrect");
                    Assert.AreEqual(p.device_id, crash.platform.device_id, "The device_id is incorrect");
                    //Assert.AreEqual("Nokia Lumia 800", crash.platform.device_model, "The device_model is incorrect");
                    //Assert.AreEqual("Windows Phone", crash.platform.os_name, "The os_name is incorrect");
                    //Assert.AreEqual("8.0", crash.platform.os_version, "The os_version is incorrect");
                    crash.DeleteFromDisk();
                } else {
                    Assert.Fail("The Crash message isn't in the queue");
                }
            }
        }

        [TestMethod]
        public void ThreadQueueManagementTest() {
            Console.WriteLine("starting ThreadQueueManagementTest");
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableCommunicationLayer = false;
            Crittercism.Init("50807ba33a47481dd5000002");
            Crittercism.LeaveBreadcrumb("Breadcrumb test");

            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                Crittercism.LogHandledException(ex);
                Crittercism.CreateCrashReport(ex);
                if (Crittercism.MessageQueue.Count == 3) {
                    // start manually the thread and ensure that the messages are consume
                    lock (Crittercism.readerThread) {
                        if (Crittercism.readerThread.ThreadState == ThreadState.Unstarted) {
                            Crittercism.readerThread.Start();
                        } else if (Crittercism.readerThread.ThreadState == ThreadState.Stopped || Crittercism.readerThread.ThreadState == ThreadState.Aborted) {
                            QueueReader queueReader = new QueueReader();
                            ThreadStart threadStart = new ThreadStart(queueReader.ReadQueue);
                            Crittercism.readerThread = new Thread(threadStart);
                            Crittercism.readerThread.Name = "Crittercism Sender";
                            Crittercism.readerThread.Start();
                        }
                    }

                    // Waiting for finish the thread
                    Crittercism.readerThread.Join();
                    if (Crittercism.MessageQueue.Count != 0) {
                        Assert.Fail("All the messages aren't consume correctly");
                    }
                } else {
                    Assert.Fail("The messages aren't in the queue");
                }
            }
        }
    }
}
