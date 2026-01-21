namespace GuildQuestsAPI.Models
{
    public class SkillNode
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public int RequiredLevel { get; set; }

        // Simple buff values
        public int StrengthBonus { get; set; }
        public int IntelligenceBonus { get; set; }
        public int AgilityBonus { get; set; }
        public int PerceptionBonus { get; set; }

        public int LuckBonus { get; set; }
    }
}
