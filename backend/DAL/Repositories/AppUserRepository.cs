using DAL.Data;
using DAL.Identity;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly AppDbContext _dbc;

        public AppUserRepository(AppDbContext dbc)
        {
            _dbc = dbc;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbc.AppUsers.AnyAsync(u => u.Username == username);
        }

        public async Task<AppUser?> GetByUsernameAsync(string username)
        {
            return await _dbc.AppUsers.FirstOrDefaultAsync(u => u.Username == username);
        }

        public void Add(AppUser user)
        {
            _dbc.AppUsers.Add(user);
        }
    }
}
