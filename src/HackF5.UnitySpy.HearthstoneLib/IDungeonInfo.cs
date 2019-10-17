namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IDungeonInfo
    {
        IReadOnlyList<int> DeckCards { get; }

        IReadOnlyList<int> DeckList { get; }

        IReadOnlyList<IReadOnlyList<int>> LootOptionBundles { get; }

        int ChosenLoot { get; }

        IReadOnlyList<int> TreasureOption { get; }

        int ChosenTreasure { get; }

        int RunActive { get; }

        int SelectedDeck { get; }
    }
}
