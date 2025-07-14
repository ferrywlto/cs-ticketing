namespace CustomerServiceApp.Infrastructure.Options;

/// <summary>
/// Configuration options for JWT token generation and validation
/// </summary>
public class JwtTokenOptions
{
    /// <summary>
    /// Secret key used for signing JWT tokens
    /// </summary>
    public required string SecretKey { get; init; }
    
    /// <summary>
    /// Token issuer
    /// </summary>
    public required string Issuer { get; init; }
    
    /// <summary>
    /// Token audience
    /// </summary>
    public required string Audience { get; init; }
    
    /// <summary>
    /// Token expiry time in minutes
    /// </summary>
    public int ExpiryMinutes { get; init; } = 60;
}
