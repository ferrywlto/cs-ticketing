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
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,
    [Required(ErrorMessage = "Password is required.")]
    string Password);
