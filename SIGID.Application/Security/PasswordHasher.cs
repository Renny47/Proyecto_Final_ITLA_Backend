using System.Security.Cryptography;
using System.Text;

namespace SIGID.Application.Security;

public interface IPasswordHasher
{
    string HashPassword(string password, string salt);
    bool VerifyPassword(string password, string salt, string hash);
    string GenerateSalt();
}

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
        var hashBytes = sha256.ComputeHash(saltedPasswordBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string salt, string hash)
    {
        var computedHash = HashPassword(password, salt);
        return computedHash == hash;
    }

    public string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
}