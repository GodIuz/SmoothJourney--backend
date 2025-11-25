using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace SmoothJourneyAPI.Services
{
    public class PasswordService
    {
        private readonly int _iterations;
        private readonly int _memoryKb;
        private readonly int _degreeOfParallelism;
        private readonly int _hashLength;

        // Accept config via ctor if you want
        public PasswordService(int iterations = 4, int memoryKb = 1024 * 64, int degreeOfParallelism = 4, int hashLength = 32)
        {
            _iterations = iterations;
            _memoryKb = memoryKb;
            _degreeOfParallelism = degreeOfParallelism;
            _hashLength = hashLength;
        }
        public (string Hash, string Salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _degreeOfParallelism,
                MemorySize = _memoryKb,
                Iterations = _iterations
            };

            var hash = argon.GetBytes(_hashLength);
            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public bool Verify(string storedHash, string password, string storedSalt)
        {
            var salt = Convert.FromBase64String(storedSalt);
            var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _degreeOfParallelism,
                MemorySize = _memoryKb,
                Iterations = _iterations
            };

            var computed = argon.GetBytes(_hashLength);
            var computedB64 = Convert.ToBase64String(computed);

            // constant time compare
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(storedHash),
                Convert.FromBase64String(computedB64)
            );
        }
    }
}
