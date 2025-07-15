using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Application.Authentication;

namespace CustomerServiceApp.Web.Models;

/// <summary>
/// Mutable login model for form binding
/// </summary>
public class LoginModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Converts to LoginRequestDto
    /// </summary>
    /// <returns>LoginRequestDto</returns>
    public LoginRequestDto ToDto() => new(Email, Password);
}
