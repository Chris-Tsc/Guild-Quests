using Microsoft.AspNetCore.Identity;
using DAL.Models;


namespace GuildQuestsAPI.Identity
{
    public class AppUser : IdentityUser
    {
        public Player? Player { get; set; }
    }
}
