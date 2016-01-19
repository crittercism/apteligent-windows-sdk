using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest {
    [TestClass]
    public class AdditionTest {
        [TestMethod]
        public void FullAdderCircuitTest() {
            Assert.AreEqual(1 + 1,2,"ALU failure #1");
            Assert.IsTrue(1 + 1 == 2,"ALU failure #2");
        }
    }
}
