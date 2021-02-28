using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoKhoomii;

namespace UTestAutoKhoomii
{
    [TestClass]
    public class UTestKhoomiiData
    {
        [TestMethod]
        public void KhoomiiData_dBToAmplitude()
        {
            float amp = KhoomiiData.dBToAmplitude(20, 1);
            Assert.AreEqual(10, amp);
        }
    }
}
