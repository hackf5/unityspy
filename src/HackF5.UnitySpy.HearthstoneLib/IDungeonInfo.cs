namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IDungeonInfo
    {
        IReadOnlyList<int> DeckList { get; }
    }
}
