using CrittercismSDK;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrittercismSDKUnitTestApp.Tests {
    [TestClass]
    public class CrittercismTests {
        [TestMethod]
        public void InitWithInvalidAppIdThrowsInvalidAppIdException() {
            Assert.ThrowsException<InvalidAppIdException>(() => Crittercism.Init("junk_appid"));
        }
    }
}
