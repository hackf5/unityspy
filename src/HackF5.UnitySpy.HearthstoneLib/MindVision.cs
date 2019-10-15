

namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Collection;
    using HackF5.UnitySpy.HearthstoneLib.Match;

    public class MindVision
    {
        private HearthstoneImage image;

        public MindVision()
        {
            var process = Process.GetProcessesByName("Hearthstone").FirstOrDefault();
            this.image = new HearthstoneImage(AssemblyImageFactory.Create(process.Id));
        }

        public List<CollectionCard> GetCollection()
        {
            return new CollectionReader(image).GetCollection();
        }

        public MatchInfo GetMatchInfo()
        {
            return new MatchReader(image).GetMatchInfo();
        }


    }
}
