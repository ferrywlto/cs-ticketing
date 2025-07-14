using System.IdentityModel.Tokens.Jwt;
using CustomerServiceApp.Application.Common.DTOs;

namespace CustomerServiceApp.Application.Common.Interfaces;

/// <summary>
/// Interface for JWT token service operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the authenticated user
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(UserDto user);
    
    /// <summary>
    /// Validates a JWT token and returns the JWT security token if valid
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>JwtSecurityToken if token is valid, null otherwise</returns>
    JwtSecurityToken? ValidateToken(string token);
    
    /// <summary>
    /// Extracts user ID from JWT token without validation
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>User ID if extraction is successful, null otherwise</returns>
    Guid? GetUserIdFromToken(string token);
    
    /// <summary>
    /// Extracts the user ID from a validated JWT security token
    /// </summary>
    /// <param name="jwtToken">The JWT security token</param>
    /// <returns>User ID if extraction is successful, null otherwise</returns>
    Guid? GetUserIdFromValidatedToken(JwtSecurityToken jwtToken);
}
