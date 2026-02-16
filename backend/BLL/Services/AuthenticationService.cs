using DAL.Data;
using DAL.Identity;
using Microsoft.EntityFrameworkCore;


namespace BLL.Services
{
    public class AuthenticationService
    {
        private readonly AppDbContext _dbc;
        private readonly JwtTokenService _jwt;

        public AuthenticationService(AppDbContext dbc, JwtTokenService jwt)
        {
            _dbc = dbc;
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

            bool exists = await _dbc.AppUsers.AnyAsync(u => u.Username == username);
            if (exists) throw new Exception("Username already exists.");

            var (hash, salt) = PasswordHasher.HashPassword(password);

            var user = new AppUser
            {
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _dbc.AppUsers.Add(user);
            await _dbc.SaveChangesAsync();

            return _jwt.CreateToken(user);
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            username = (username ?? string.Empty).Trim();
            password = password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new Exception("Invalid credentials.");

            var user = await _dbc.AppUsers.FirstOrDefaultAsync(u => u.Username == username);
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
