using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoKhoomii;

namespace UTestAutoKhoomii
{
    [TestClass]
    public class KhoomiiPlayerUTest
    {
        [TestMethod]
        public void KhoomiiPlayer_LoadKhoomiiFrequency_1()
        {
            KhoomiiPlayer khoomiiPlayer = new KhoomiiPlayer();
            try{
                List<KhoomiiData> khoomiiDatas = khoomiiPlayer.LoadKhoomiiFrequency(@"E:/Owner/VS2017/Repos/AutoKhoomii/data/KhoomiiFrequency.json");
                Assert.AreEqual("Pattern1", khoomiiDatas[0].Name);
                Assert.AreEqual(430, khoomiiDatas[0].FrequencyInfos[0].Frequency);
            } catch (Exception e){
                Console.WriteLine(e.Message);
                Assert.Fail();
            }
        }
    }
}
