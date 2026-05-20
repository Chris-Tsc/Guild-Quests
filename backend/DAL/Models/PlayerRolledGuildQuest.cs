

namespace DAL.Models
{
    public class PlayerRolledGuildQuest
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        public int GuildQuestId { get; set; }
        public GuildQuest? GuildQuest { get; set; }

        public DateTime DaytimeInfoUtc { get; set; }
    }
}
