using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Application.Common.DTOs;

namespace CustomerServiceApp.Application.Authentication;

/// <summary>
/// Authentication result record containing user information and JWT token
/// </summary>
public record AuthenticationResultDto(
    UserDto User,
    string Token,
    DateTime ExpiresAt);

/// <summary>
/// Login request record
/// </summary>
public record LoginRequestDto(
    [property: Required(ErrorMessage = "Email is required.")]
    [property: EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,
    [property: Required(ErrorMessage = "Password is required.")]
    string Password);
