using System.ComponentModel.DataAnnotations;

namespace CustomerServiceApp.Domain.Users;

public abstract class User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Name is required.")]
    public required string Name { get; init; }

    public string? Avatar { get; init; }

    [Required(ErrorMessage = "Password hash is required.")]
    public required string PasswordHash { get; init; }

    public abstract UserType UserType { get; }

    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}
