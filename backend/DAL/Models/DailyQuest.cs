namespace DAL.Models
{
    public class DailyQuest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int BaseXP { get; set; }

        // Reference Events
        public int EventsId { get; set; }
        public Events? Events { get; set; }

        public int RequiredLevel { get; set; } // Hidden until player reaches this level
    }
}
