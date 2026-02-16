using System.Security.Cryptography;

namespace BLL.Services
{
    public static class PasswordHasher
    {
        public static (string hashBase64, string saltBase64) HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA512);

            byte[] hash = pbkdf2.GetBytes(32);

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool VerifyPassword(string password, string storedHashBase64, string storedSaltBase64)
        {
            byte[] salt = Convert.FromBase64String(storedSaltBase64);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA512);

            byte[] hash = pbkdf2.GetBytes(32);
            byte[] storedHash = Convert.FromBase64String(storedHashBase64);

            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
