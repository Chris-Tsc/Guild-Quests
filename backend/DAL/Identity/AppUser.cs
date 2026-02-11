using Microsoft.AspNetCore.Identity;
using DAL.Models;


namespace DAL.Identity
{
    public class AppUser : IdentityUser
    {
        public Player? Player { get; set; }
    }
}
