using DAL.Models.Enums;

namespace DAL.Models
{
    public class QuestOption
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

        public int QuestId { get; set; }

        public QuestTypeEnum? QuestType { get; set; }
        // Daily or Guild
    }
}
