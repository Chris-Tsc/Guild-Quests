using DAL.Identity;

namespace DAL.Repositories.Interfaces
{
    public interface IAppUserRepository
    {
        Task<bool> UsernameExistsAsync(string username);
        Task<AppUser?> GetByUsernameAsync(string username);
        void Add(AppUser user);
    }
}
