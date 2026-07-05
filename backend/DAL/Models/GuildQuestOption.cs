namespace DAL.Models
{
    public class GuildQuestOption
    {
        public int Id { get; set; }

        public string? Text { get; set; }

        public int StrengthWeight { get; set; }
        public int IntelligenceWeight { get; set; }
        public int AgilityWeight { get; set; }
        public int PerceptionWeight { get; set; }

        public int BaseSuccessChance { get; set; }

        public int GuildQuestId { get; set; }
        public GuildQuest? GuildQuest { get; set; }
    }
}
