using DAL.Models.Enums;

namespace DAL.Models
{

    public class GuildQuest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public GuildQuestEnum? GuildQuestType { get; set; }

        // Reference Events
        public int EventsId { get; set; }
        public Events? Events { get; set; }

        public int RequiredLevel { get; set; } = 1;  // Hidden until player reaches this level
        public int EnergyCost { get; set; } = 1;     // Energy needed to accept
    }
}