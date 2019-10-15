namespace HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo
{
    using System.Collections.Generic;

    public class DungeonInfo : IDungeonInfo
    {
        public IReadOnlyList<int> DeckList { get; set; }
    }
}
