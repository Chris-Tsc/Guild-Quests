namespace GuildQuestsAPI.Models
{
    public enum EventCategory
    {
        Monster = 1,
        Farming = 2,
        Mining = 3,
        Exploration = 4
    }

    public class Events
    {
        public int Id { get; set; }
        public string Name { get; set; }               
        public EventCategory Category { get; set; }    
        public int BaseDifficulty { get; set; }   // Base difficulty of this event
        public double ScalingFactor { get; set; } // How difficulty scales with player level
        public int BaseXP { get; set; } 
    }
}

