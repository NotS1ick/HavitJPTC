using System.Security.Cryptography;

namespace Havit.Services.Utilities;

public class PasswordHasher : IPasswordHasher
{
    private readonly HashAlgorithmName _algorithm;
    private readonly int _iterations;
    private readonly int _keySize = 32;
    private readonly string _pepper = "JPTC2024"; // Really couldn't have been asked to store this in .ENV
    private readonly int _saltSize = 16;

    public PasswordHasher(IConfiguration configuration)
    {
        _iterations = configuration.GetValue("PasswordHasher:Iterations", 100000);
        _algorithm = HashAlgorithmName.SHA256;
    }

    public string HashPassword(string password)
    {
        var passwordWithPepper = password + _pepper;

        using (var rng = RandomNumberGenerator.Create())
        {
            var salt = new byte[_saltSize];
            rng.GetBytes(salt);

            var key = Rfc2898DeriveBytes.Pbkdf2(
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
        if (parts.Length != 3) return false;

        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var hash = Convert.FromBase64String(parts[2]);

        var providedPasswordWithPepper = providedPassword + _pepper;

        var providedHash = Rfc2898DeriveBytes.Pbkdf2(
            providedPasswordWithPepper,
            salt,
            iterations,
            _algorithm,
            hash.Length
        );

        return CryptographicOperations.FixedTimeEquals(providedHash, hash);
    }
}