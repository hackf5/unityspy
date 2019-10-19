namespace HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo
{
    using System.Collections.Generic;

    internal class DungeonInfo : IDungeonInfo
    {
        public DungeonKey Key { get; set; }

        public IReadOnlyList<int> DeckCards { get; set; }

        public IReadOnlyList<int> DeckList { get; set; }

        public IReadOnlyList<IReadOnlyList<int>> LootOptionBundles { get; set; }

        public int ChosenLoot { get; set; }

        public IReadOnlyList<int> TreasureOption { get; set; }

        public int ChosenTreasure { get; set; }

        public int RunActive { get; set; }

        public int SelectedDeck { get; set; }

        public int StartingTreasure { get; set; }
    }
}
