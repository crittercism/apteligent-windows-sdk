using CrittercismSDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest {
    [TestClass]
    public class HandledExceptionTests {
        [TestCleanup()]
        public void TestCleanup() {
            // Use TestCleanup to run code after each test has run
            Crittercism.Shutdown();
            TestHelpers.Cleanup();
        }
        [TestMethod]
        public void HandledExceptionTest() {
            TestHelpers.StartApp();
            TestHelpers.LogHandledException();
            MessageReport messageReport=TestHelpers.DequeueMessageType(typeof(HandledException));
            Assert.IsNotNull(messageReport,"Expected a HandledException message");
        }
    }
}
