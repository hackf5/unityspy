namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;

    public interface IDeck
    {
        string Name { get; }

        IReadOnlyList<int> DeckList { get; }
    }
}
