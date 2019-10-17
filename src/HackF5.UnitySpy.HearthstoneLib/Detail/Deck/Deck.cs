namespace HackF5.UnitySpy.HearthstoneLib.Detail.Deck
{
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class Deck : IDeck
    {
        public string Name { get; set; }

        public IReadOnlyList<int> DeckList { get; set; }
    }
}
