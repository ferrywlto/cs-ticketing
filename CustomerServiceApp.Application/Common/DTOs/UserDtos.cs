using System.ComponentModel.DataAnnotations;

namespace CustomerServiceApp.Application.Common.DTOs;

/// <summary>
/// Base DTO for user entities
/// </summary>
public abstract class UserDto
{
    public Guid Id { get; init; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "Name is required.")]
    public required string Name { get; init; }
    
    public string? Avatar { get; init; }
    
    public abstract string UserType { get; }
}

/// <summary>
/// DTO for Player entities
/// </summary>
public class PlayerDto : UserDto
{
    [Required(ErrorMessage = "Player number is required.")]
    public required string PlayerNumber { get; init; }
    
    public override string UserType => "Player";
}

/// <summary>
/// DTO for Agent entities
/// </summary>
public class AgentDto : UserDto
{
    public override string UserType => "Agent";
}

/// <summary>
/// DTO for creating new user entities
/// </summary>
public abstract class CreateUserDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "Name is required.")]
    public required string Name { get; init; }
    
    public string? Avatar { get; init; }
    
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public required string Password { get; init; }
}

/// <summary>
/// DTO for creating new Player entities
/// </summary>
public class CreatePlayerDto : CreateUserDto
{
    [Required(ErrorMessage = "Player number is required.")]
    public required string PlayerNumber { get; init; }
}

/// <summary>
/// DTO for creating new Agent entities
/// </summary>
public class CreateAgentDto : CreateUserDto
{
}

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
