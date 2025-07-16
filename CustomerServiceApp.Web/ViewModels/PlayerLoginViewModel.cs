using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Application.Authentication;

namespace CustomerServiceApp.Web.ViewModels;

/// <summary>
/// View model for player login form with validation and state management
/// </summary>
public class PlayerLoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }

    /// <summary>
    /// Converts the view model to LoginRequestDto for API calls
    /// </summary>
    /// <returns>LoginRequestDto with current email and password values</returns>
    public LoginRequestDto ToLoginRequest() => new(Email, Password);

    /// <summary>
    /// Resets the form state to initial values
    /// </summary>
    public void Reset()
    {
        Email = string.Empty;
        Password = string.Empty;
        ErrorMessage = null;
        IsLoading = false;
    }

    /// <summary>
    /// Sets loading state and clears any existing error message
    /// </summary>
    /// <param name="loading">The loading state to set</param>
    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        if (loading)
        {
            ErrorMessage = null;
        }
    }

    /// <summary>
    /// Sets an error message and ensures loading is disabled
    /// </summary>
    /// <param name="message">The error message to display</param>
    public void SetError(string message)
    {
        ErrorMessage = message;
        IsLoading = false;
    }
}
