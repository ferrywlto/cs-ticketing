namespace CustomerServiceApp.Application.Common.Interfaces;

/// <summary>
/// Interface for password hashing and verification
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using a secure hashing algorithm
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">The plain text password to verify</param>
    /// <param name="hashedPassword">The hashed password to compare against</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
