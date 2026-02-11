namespace DAL.Models
{
    public class PlayerGuildQuest
    {
        public int Id { get; set; }
        public Player? Player { get; set; }
        public int PlayerId { get; set; }
        public GuildQuest? GuildQuest { get; set; }
        public int GuildQuestId { get; set; }
        public bool IsCompleted { get; set; }
    }
}
