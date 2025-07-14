using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Application.Common.DTOs;

namespace CustomerServiceApp.Application.Authentication;

/// <summary>
/// Authentication result containing user information and JWT token
/// </summary>
public class AuthenticationResultDto
{
    public required UserDto User { get; init; }
    public required string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "Password is required.")]
    public required string Password { get; init; }
}
