using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Models;

namespace CustomerServiceApp.Application.Authentication;

/// <summary>
/// Interface for authentication service operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user and returns JWT token
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>Authentication result with user info and token</returns>
    Task<Result<AuthenticationResultDto>> LoginAsync(LoginRequestDto loginRequest);
    
    /// <summary>
    /// Validates a JWT token and returns user information
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>User information if token is valid</returns>
    Task<Result<UserDto>> ValidateTokenAsync(string token);
}
