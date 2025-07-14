using System.ComponentModel.DataAnnotations;

namespace CustomerServiceApp.Infrastructure.Options;

/// <summary>
/// Configuration options for password hashing
/// </summary>
public class PasswordHasherOptions
{
    public const string SectionName = "PasswordHasher";

    /// <summary>
    /// Salt value used for password hashing. Should be stored securely in user secrets.
    /// </summary>
    public string Salt { get; init; } = string.Empty;

    /// <summary>
    /// Number of iterations for PBKDF2 (default: 100,000)
    /// </summary>
    [Range(10000, 1000000, ErrorMessage = "Iterations must be between 10,000 and 1,000,000.")]
    public int Iterations { get; init; } = 100000;
}
