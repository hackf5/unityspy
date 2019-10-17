namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IMatchInfo
    {
        int BrawlSeasonId { get; }

        GameFormat FormatType { get; }

        GameType GameType { get; }

        IPlayer LocalPlayer { get; }

        int MissionId { get; }

        IPlayer OpposingPlayer { get; }

        int RankedSeasonId { get; }

        bool Spectator { get; }
    }
}