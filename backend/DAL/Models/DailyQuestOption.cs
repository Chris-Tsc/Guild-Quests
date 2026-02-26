namespace DAL.Models
{
    public class DailyQuestOption
    {
        public int Id { get; set; }

        public string? Text { get; set; }

        // Stat influence on success chance
        public int StrengthWeight { get; set; }
        public int IntelligenceWeight { get; set; }
        public int AgilityWeight { get; set; }
        public int PerceptionWeight { get; set; }

        public int BaseSuccessChance { get; set; }

        // Relationships 

        public int DailyQuestId { get; set; }

        public DailyQuest? DailyQuest { get; set; }        
    }
}
