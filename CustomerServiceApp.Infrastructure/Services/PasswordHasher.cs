using System.Security.Cryptography;
using System.Text;
using CustomerServiceApp.Application.Common.Interfaces;

namespace CustomerServiceApp.Infrastructure.Services;

/// <summary>
/// Implementation of password hashing using SHA256 (for demo purposes)
/// In production, use BCrypt, Argon2, or similar secure hashing algorithms
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }
}
