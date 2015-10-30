using CrittercismSDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest {
    [TestClass]
    public class CrittercismTests {
        [TestMethod]
        public void InitWithInvalidAppIdTest() {
            Crittercism.Init("junk_appid");
            Assert.IsFalse(Crittercism.initialized);
        }
    }
}
