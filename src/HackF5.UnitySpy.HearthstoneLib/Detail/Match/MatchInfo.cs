namespace HackF5.UnitySpy.HearthstoneLib.Detail.Match
{
    internal class MatchInfo : IMatchInfo
    {
        public int BrawlSeasonId { get; set; }

        public int FormatType { get; set; }

        public int GameType { get; set; }

        public IPlayer LocalPlayer { get; set; }

        public int MissionId { get; set; }

        public IPlayer OpposingPlayer { get; set; }

        public int RankedSeasonId { get; set; }

        public bool Spectator { get; set; }
    }
}