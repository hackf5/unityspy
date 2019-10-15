namespace HackF5.UnitySpy.HearthstoneLib.Match
{
    public class MatchInfo
    {
        public Player LocalPlayer { get; set; }

        public Player OpposingPlayer { get; set; }

        public int BrawlSeasonId { get; set; }

        public int MissionId { get; set; }

        public int RankedSeasonId { get; set; }

        public int GameType { get; set; }

        public int FormatType { get; set; }

        public bool Spectator { get; set; }        
    }
}
