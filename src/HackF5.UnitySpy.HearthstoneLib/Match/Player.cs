namespace HackF5.UnitySpy.HearthstoneLib.Match
{
    public class Player
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public int StandardRank { get; set; }

        public int StandardLegendRank { get; set; }

        public int StandardStars { get; set; }

        public int WildRank { get; set; }

        public int WildLegendRank { get; set; }

        public int WildStars { get; set; }

        public int CardBackId { get; set; }

        public Account Account { get; set; }

        public BattleTag BattleTag { get; set; }
    }
}
