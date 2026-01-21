using static GuildQuestsAPI.Models.Events;

namespace GuildQuestsAPI.Models
{
    public enum GuildQuestType
    {
        Normal = 1,
        Elite = 2
    }

    public class GuildQuest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public GuildQuestType Type { get; set; }

        // Reference Events
        public int EventsId { get; set; }
        public Events Events { get; set; }

        public int RequiredLevel { get; set; } = 1;  // Hidden until player reaches this level
        public int EnergyCost { get; set; } = 1;     // Energy needed to accept
    }
}