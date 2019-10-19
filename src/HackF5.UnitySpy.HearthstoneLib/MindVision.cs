﻿namespace HackF5.UnitySpy.HearthstoneLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Collection;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Match;

    public class MindVision
    {
        private HearthstoneImage Image;

        public MindVision()
        {
            var process = Process.GetProcessesByName("Hearthstone").FirstOrDefault();
            if (process == null)
            {
                throw new InvalidOperationException(
                    "Failed to find Hearthstone executable. Please check that Hearthstone is running.");
            }

            this.Image = new HearthstoneImage(AssemblyImageFactory.Create(process.Id));
        }

        public IReadOnlyList<ICollectionCard> GetCollection() => CollectionReader.GetCollection(this.Image);

        public IFullDungeonInfo GetDungeonInfo() => DungeonInfoReader.GetFullDungeonInfo(this.Image);

        public IMatchInfo GetMatchInfo() => MatchReader.GetMatchInfo(this.Image);
    }
}