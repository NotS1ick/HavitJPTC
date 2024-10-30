using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Havit.Services.Utilities
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly string _pepper = "JPTC2024"; // Really couldn't have been asked to store this in .ENV
        private readonly int _saltSize = 16;
        private readonly int _keySize = 32;
        private readonly int _iterations;
        private readonly HashAlgorithmName _algorithm;

        public PasswordHasher(IConfiguration configuration)
        {
            _iterations = configuration.GetValue<int>("PasswordHasher:Iterations", 100000);
            _algorithm = HashAlgorithmName.SHA256;
        }

        public string HashPassword(string password)
        {
            string passwordWithPepper = password + _pepper;

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[_saltSize];
                rng.GetBytes(salt);

                byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                    passwordWithPepper,
                    salt,
                    _iterations,
                    _algorithm,
                    _keySize
                );

                return $"{_iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
            }
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] hash = Convert.FromBase64String(parts[2]);

            string providedPasswordWithPepper = providedPassword + _pepper;

            byte[] providedHash = Rfc2898DeriveBytes.Pbkdf2(
                providedPasswordWithPepper,
                salt,
                iterations,
                _algorithm,
                hash.Length
            );

            return CryptographicOperations.FixedTimeEquals(providedHash, hash);
        }
    }
}