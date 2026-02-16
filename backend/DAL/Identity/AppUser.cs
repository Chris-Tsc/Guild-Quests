using DAL.Models;
using System.ComponentModel.DataAnnotations;


namespace DAL.Identity
{
    public class AppUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); 

        [Required]
        public string? Username { get; set; }             

        [Required]
        public string? PasswordHash { get; set; }          

        [Required]
        public string? PasswordSalt { get; set; }          
        public Player? Player { get; set; }
    }
}
