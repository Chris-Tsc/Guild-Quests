using DAL.Models;
using System.ComponentModel.DataAnnotations;


namespace DAL.Identity
{
    public class AppUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Username { get; set; } = null!;         

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string PasswordSalt { get; set; } = null!;
        public Player? Player { get; set; }
    }
}
