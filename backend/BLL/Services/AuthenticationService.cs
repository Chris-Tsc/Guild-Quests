using BLL.Services.Interfaces;
using DAL.Identity;
using DAL.Repositories.Interfaces;


namespace BLL.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAppUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _jwt;

        public AuthenticationService(IAppUserRepository users, IUnitOfWork unitOfWork, JwtTokenService jwt)
        {
            _users = users;
            _unitOfWork = unitOfWork;
            _jwt = jwt;
        }

        public async Task<string> RegisterAsync(string username, string password)
        {
            username = (username ?? string.Empty).Trim();
            password = password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("Username is required.");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Password is required.");

            bool exists = await _users.UsernameExistsAsync(username);
            if (exists) throw new Exception("Username already exists.");

            var (hash, salt) = PasswordHasher.HashPassword(password);

            var user = new AppUser
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _users.Add(user);
            await _unitOfWork.SaveChangesAsync();

            return _jwt.CreateToken(user);
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            username = (username ?? string.Empty).Trim();
            password = password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new Exception("Invalid credentials.");

            var user = await _users.GetByUsernameAsync(username);
            if (user == null) throw new Exception("Invalid credentials.");

            var storedHash = user.PasswordHash ?? string.Empty;
            var storedSalt = user.PasswordSalt ?? string.Empty;

            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                throw new Exception("Invalid credentials.");

            bool ok = PasswordHasher.VerifyPassword(password, storedHash, storedSalt);
            if (!ok) throw new Exception("Invalid credentials.");

            return _jwt.CreateToken(user);
        }
    }
}
