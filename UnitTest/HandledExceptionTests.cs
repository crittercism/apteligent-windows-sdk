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
        [TestMethod]
        public void HandledExceptionTest() {
            TestHelpers.StartApp();
            TestHelpers.LogHandledException();
            MessageReport messageReport=TestHelpers.DequeueMessageType(typeof(HandledException));
            Assert.IsNotNull(messageReport,"Expected a HandledException message");
        }
    }
}
