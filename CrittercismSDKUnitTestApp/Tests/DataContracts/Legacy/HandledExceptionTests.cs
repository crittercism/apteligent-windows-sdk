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
    }
}
