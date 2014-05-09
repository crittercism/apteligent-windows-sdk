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
    public class QueueManagementTests {
        [TestMethod]
        public void AppLoadQueueManagementTest() {
            TestHelpers.InitializeLeaveLoadOnQueue(TestHelpers.VALID_APPID);

            Assert.AreEqual(1, Crittercism.MessageQueue.Count);
            AppLoad appLoad = Crittercism.MessageQueue.Dequeue() as AppLoad;
            try {
                Assert.IsNotNull(appLoad, "The message isn´t AppLoad type");
                Platform p = new Platform();
                Assert.AreEqual(TestHelpers.VALID_APPID, appLoad.appLoads.appID, "The app_id is incorrect");
            } finally {
                appLoad.DeleteFromDisk();
            }
        }

        [TestMethod]
        public void HandledExceptionQueueManagementTest() {
            TestHelpers.InitializeLeaveLoadOnQueue(TestHelpers.VALID_APPID);

            try {
                TestHelpers.ThrowDivideByZeroException();
            } catch (Exception ex) {
                // Create the error message
                Crittercism.LogHandledException(ex);
                Assert.AreEqual(Crittercism.MessageQueue.Count, 1);
                HandledException he = Crittercism.MessageQueue.Dequeue() as HandledException;
                try {
                    // verify that the message in the queue is a HandledException type
                    Assert.IsNotNull(he, "The message isn't HandledException type");

                    // verify each field of the error message
                    Assert.AreEqual(TestHelpers.VALID_APPID, he.app_id, "The app_id is incorrect");
                    Assert.AreEqual(System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString(), he.app_state["app_version"], "The app_version is incorrect");
                    Assert.AreEqual(ex.GetType().FullName, he.error.name, "The error name is incorrect");
                    Assert.AreEqual(ex.Message, he.error.reason, "The error reason is incorrect");
                    List<string> stackTraceList = ex.StackTrace.Split(new string[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (int index = 0; index < stackTraceList.Count; index++) {
                        Assert.AreEqual(stackTraceList[index], he.error.stack_trace[index], "The stack_trace is incorrect");
                    }

                    Platform p = new Platform();
                    Assert.AreEqual(p.device_id, he.platform.device_id, "The device_id is incorrect");
                } finally {
                    he.DeleteFromDisk();
                }
            }
        }

        [TestMethod]
        public void CrashQueueManagementTest() {
            TestHelpers.InitializeRemoveLoadFromQueue(TestHelpers.VALID_APPID);

            int i = 0;
            int j = 5;
            try {
                int k = j / i;
            } catch (Exception ex) {
                Crittercism.LeaveBreadcrumb("Breadcrumb test");

                Crittercism.CreateCrashReport(ex);
                Assert.AreEqual(Crittercism.MessageQueue.Count, 1);
                Crash crash = Crittercism.MessageQueue.Dequeue() as Crash;

                try {
                    Assert.IsNotNull(crash, "The message isn't Crash type");

                    Assert.AreEqual(TestHelpers.VALID_APPID, crash.app_id, "The app_id is incorrect");
                    Assert.AreEqual(System.Windows.Application.Current.GetType().Assembly.GetName().Version.ToString(), crash.app_state["app_version"], "The app_version is incorrect");
                    Assert.AreEqual("Breadcrumb test", crash.breadcrumbs.current_session[0].message, "The breadcrumb message is incorrect");
                    Assert.AreEqual(ex.GetType().FullName, crash.crash.name, "The crash name is incorrect");
                    Assert.AreEqual(ex.Message, crash.crash.reason, "The crash reason is incorrect");
                    List<string> stackTraceList = ex.StackTrace.Split(new string[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (int index = 0; index < stackTraceList.Count; index++) {
                        Assert.AreEqual(stackTraceList[index], crash.crash.stack_trace[index], "The stack_trace is incorrect");
                    }

                    Platform p = new Platform();
                    Assert.AreEqual(p.device_id, crash.platform.device_id, "The device_id is incorrect");
                } finally {
                    crash.DeleteFromDisk();
                }
            }
        }

        [TestMethod]
        public void ThreadQueueManagementTest() {
            Crittercism._autoRunQueueReader = false;
            Crittercism._enableCommunicationLayer = false;
            Crittercism.Init(TestHelpers.VALID_APPID);
            Crittercism.LeaveBreadcrumb("Breadcrumb test");

            var ex = new Exception();
            Crittercism.LogHandledException(ex);
            Crittercism.CreateCrashReport(ex);
                
            Assert.AreEqual(2, Crittercism.MessageQueue.Count == 2);
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

            Crittercism.readerThread.Join();
            Assert.AreNotEqual(0, Crittercism.MessageQueue.Count);
        }
    }
}
