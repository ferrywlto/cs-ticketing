using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerServiceApp.Application.Common.DTOs;

/// <summary>
/// Base DTO record for user entities
/// </summary>
[JsonDerivedType(typeof(PlayerDto), typeDiscriminator: "Player")]
[JsonDerivedType(typeof(AgentDto), typeDiscriminator: "Agent")]
public abstract record UserDto(
    Guid Id,
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,
    [Required(ErrorMessage = "Name is required.")]
    string Name,
    string? Avatar = null)
{
    public abstract string UserType { get; }
}

/// <summary>
/// DTO record for Player entities
/// </summary>
public record PlayerDto(
    Guid Id,
    string Email,
    string Name,
    [Required(ErrorMessage = "Player number is required.")]
    string PlayerNumber,
    string? Avatar = null) : UserDto(Id, Email, Name, Avatar)
{
    public override string UserType => "Player";
}

/// <summary>
/// DTO record for Agent entities
/// </summary>
public record AgentDto(
    Guid Id,
    string Email,
    string Name,
    string? Avatar = null) : UserDto(Id, Email, Name, Avatar)
{
    public override string UserType => "Agent";
}

/// <summary>
/// DTO record for creating new user entities
/// </summary>
public abstract record CreateUserDto(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,
    [Required(ErrorMessage = "Name is required.")]
    string Name,
    string? Avatar,
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    string Password);

/// <summary>
/// DTO record for creating new Player entities
/// </summary>
public record CreatePlayerDto(
    string Email,
    string Name,
    string? Avatar,
    string Password,
    [Required(ErrorMessage = "Player number is required.")]
    string PlayerNumber) : CreateUserDto(Email, Name, Avatar, Password);

/// <summary>
/// DTO record for creating new Agent entities
/// </summary>
public record CreateAgentDto(
    string Email,
    string Name,
    string? Avatar,
    string Password) : CreateUserDto(Email, Name, Avatar, Password);

/// <summary>
/// DTO for updating user information
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// The user's email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    /// <summary>
    /// The user's first name
    /// </summary>
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }

    /// <summary>
    /// The user's last name
    /// </summary>
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }

    /// <summary>
    /// The user's password (optional for updates)
    /// </summary>
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string? Password { get; set; }
}
