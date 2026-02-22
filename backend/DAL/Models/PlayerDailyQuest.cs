namespace DAL.Models
{
    public class PlayerDailyQuest
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player? Player { get; set; }

        public int DailyQuestId { get; set; }
        public DailyQuest? DailyQuest { get; set; }

        public DateTime DaytimeInfoUtc { get; set; }

        public bool IsCompleted { get; set; }
    }
}
