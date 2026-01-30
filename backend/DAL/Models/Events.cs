using DAL.Models.Enums;

namespace DAL.Models
{
    public class Events
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public EventEnum? EventCategory { get; set; }
        //public IEnumerable<EventEnum> EventCategory { get; set; } = Enumerable.Empty<EventEnum>();
        public int BaseDifficulty { get; set; }   // Base difficulty of this event
        public double ScalingFactor { get; set; } // How difficulty scales with player level
        public int BaseXP { get; set; } 
    }
}

