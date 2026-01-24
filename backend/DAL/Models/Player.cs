namespace DAL.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int CurrentXP { get; set; }
        public int Energy { get; set; }

        // Stats
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Agility { get; set; }
        public int Perception { get; set; }
        public int Luck { get; set; }
    }
}
