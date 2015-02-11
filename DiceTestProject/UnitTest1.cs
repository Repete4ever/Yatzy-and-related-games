using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yatzy;

namespace DiceTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestPower()
        {
            Assert.AreEqual(FiveDice.power(6, 0), 1);
            Assert.AreEqual(FiveDice.power(5, 1), 5);
            Assert.AreEqual(FiveDice.power(6, 1), 6);
            Assert.AreEqual(FiveDice.power(4, 2), 16);
            Assert.AreEqual(FiveDice.power(6, 2), 36);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_single_itoa_exception()
        {
            FiveDice.MyItoa(-1, 5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_single_itoa_exception2()
        {
            FiveDice.MyItoa(16, 5);
        }

        [TestMethod]
        public void Test_single_itoa()
        {
            Assert.AreEqual(FiveDice.MyItoa(0, 5), "0");
            Assert.AreEqual(FiveDice.MyItoa(1, 5), "1");
        }

    }
}
