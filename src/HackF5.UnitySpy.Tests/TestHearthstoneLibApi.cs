

namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    // This needs to be run while Hearthstone is running
    [TestClass]
    public class TestHearthstoneLibApi
    {
        [TestMethod]
        public void TestRetrieveCollection()
        {
            var collection = new MindVision().GetCollection();
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection.Count > 0, "collection should not be empty");
            Console.WriteLine("Collection had " + collection.Count + " cards");
        }

        // You need to have a game running for this
        [TestMethod]
        public void TestRetrieveMatchInfo()
        {
            var matchInfo = new MindVision().GetMatchInfo();
            Assert.IsNotNull(matchInfo);
            Console.WriteLine(matchInfo.LocalPlayer.StandardRank);
        }
    }
}
