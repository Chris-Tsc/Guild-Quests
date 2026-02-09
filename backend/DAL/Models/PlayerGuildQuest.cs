namespace DAL.Models
{
    public class PlayerGuildQuest
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int GuildQuestId { get; set; }
        public bool IsCompleted { get; set; }
    }
}
