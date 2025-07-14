using System.Security.Cryptography;
using System.Text;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CustomerServiceApp.Infrastructure.Services;

/// <summary>
/// Implementation of password hashing using PBKDF2 with salt for enhanced security
/// Uses user secrets to store the salt securely
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasherOptions _options;
    private readonly byte[] _salt;

    public PasswordHasher(IOptions<PasswordHasherOptions> options)
    {
        _options = options.Value;
        _salt = Encoding.UTF8.GetBytes(_options.Salt);
    }

    public string HashPassword(string password)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, _salt, _options.Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32); // 256 bits
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }
}
