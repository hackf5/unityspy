namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // This needs to be run while Hearthstone is running
    [TestClass]
    public class TestHearthstoneLibApi
    {
        [PublicAPI]
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void TestRetrieveCollection()
        {
            var collection = new MindVision().GetCollectionCards();
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection.Count > 0, "Collection should not be empty.");
            this.TestContext.WriteLine($"Collection has {collection.Count} cards.");
        }

        // You need to have a game running for this
        [TestMethod]
        public void TestRetrieveMatchInfo()
        {
            var matchInfo = new MindVision().GetMatchInfo();
            Assert.IsNotNull(matchInfo);
            this.TestContext.WriteLine($"Local player's standard rank is {matchInfo.LocalPlayer.StandardRank}.");
        }

        // You need to have a solo run (Dungeon Run, Monster Hunt, Rumble Run, Dalaran Heist, Tombs of Terror) ongoing
        [TestMethod]
        public void TestRetrieveFullDungeonInfo()
        {
            var dungeonInfo = new MindVision().GetDungeonInfoCollection();
            Assert.IsNotNull(dungeonInfo);
        }
    }
}