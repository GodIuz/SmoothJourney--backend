using System.Security.Cryptography;
using System.Text;

namespace SmoothJourneyAPI.Services
{
    public class PasswordService
    {
        public (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);

            var argon = new Konscious.Security.Cryptography.Argon2id(
                Encoding.UTF8.GetBytes(password))
            {
                Salt = saltBytes,
                DegreeOfParallelism = 4,
                MemorySize = 1024 * 64, // 64MB
                Iterations = 4
            };

            var hashBytes = argon.GetBytes(32);
            var hash = Convert.ToBase64String(hashBytes);
            var salt = Convert.ToBase64String(saltBytes);

            return (hash, salt);
        }

        public bool Verify(string storedHash, string password, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);

            var argon = new Konscious.Security.Cryptography.Argon2id(
               Encoding.UTF8.GetBytes(password))
            {
                Salt = saltBytes,
                DegreeOfParallelism = 4,
                MemorySize = 1024 * 64,
                Iterations = 4
            };

            var hashBytes = argon.GetBytes(32);
            var computed = Convert.ToBase64String(hashBytes);

            return computed == storedHash;
        }
    }
}
