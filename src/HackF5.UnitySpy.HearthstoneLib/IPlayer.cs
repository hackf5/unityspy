namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IPlayer
    {
        string Name { get; }

        int Id { get; }

        int StandardRank { get; }

        int StandardLegendRank { get; }

        int StandardStars { get; }

        int WildRank { get;  }

        int WildLegendRank { get; }

        int WildStars { get; }

        int CardBackId { get; }

        IAccount Account { get; }

        IBattleTag BattleTag { get; }
    }
}