using CustomerServiceApp.Web.ViewModels;
using CustomerServiceApp.Application.Authentication;

namespace CustomerServiceApp.UnitTests.Web.ViewModels;

public class PlayerLoginViewModelTests
{
    [Fact]
    public void ToLoginRequest_ShouldReturnCorrectDto()
    {
        var viewModel = new PlayerLoginViewModel
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var result = viewModel.ToLoginRequest();

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("password123", result.Password);
    }

    [Fact]
    public void Reset_ShouldClearAllFields()
    {
        var viewModel = new PlayerLoginViewModel
        {
            Email = "test@example.com",
            Password = "password123",
            ErrorMessage = "Some error",
            IsLoading = true
        };

        viewModel.Reset();

        Assert.Equal(string.Empty, viewModel.Email);
        Assert.Equal(string.Empty, viewModel.Password);
        Assert.Null(viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public void SetLoading_WithTrue_ShouldSetLoadingAndClearError()
    {
        var viewModel = new PlayerLoginViewModel
        {
            ErrorMessage = "Previous error",
            IsLoading = false
        };

        viewModel.SetLoading(true);

        Assert.True(viewModel.IsLoading);
        Assert.Null(viewModel.ErrorMessage);
    }

    [Fact]
    public void SetLoading_WithFalse_ShouldDisableLoading()
    {
        var viewModel = new PlayerLoginViewModel
        {
            ErrorMessage = "Existing error",
            IsLoading = true
        };

        viewModel.SetLoading(false);

        Assert.False(viewModel.IsLoading);
        Assert.Equal("Existing error", viewModel.ErrorMessage); // Should not clear existing error
    }

    [Fact]
    public void SetError_ShouldSetErrorMessageAndDisableLoading()
    {
        const string errorMessage = "Test error message";
        var viewModel = new PlayerLoginViewModel
        {
            IsLoading = true
        };

        viewModel.SetError(errorMessage);

        Assert.Equal(errorMessage, viewModel.ErrorMessage);
        Assert.False(viewModel.IsLoading);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Email_Validation_ShouldWork(string email)
    {
        var viewModel = new PlayerLoginViewModel { Email = email };
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(viewModel) { MemberName = nameof(viewModel.Email) };
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(
            viewModel.Email, validationContext, validationResults);

        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.ErrorMessage!.Contains("required") || r.ErrorMessage!.Contains("format"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345")]
    public void Password_Validation_ShouldRequireMinimumLength(string password)
    {
        var viewModel = new PlayerLoginViewModel { Password = password };
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(viewModel) { MemberName = nameof(viewModel.Password) };
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(
            viewModel.Password, validationContext, validationResults);

        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.ErrorMessage!.Contains("required") || r.ErrorMessage!.Contains("6 characters"));
    }

    [Fact]
    public void ValidModel_ShouldPassValidation()
    {
        var viewModel = new PlayerLoginViewModel
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(viewModel);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            viewModel, validationContext, validationResults, true);

        Assert.True(isValid);
        Assert.Empty(validationResults);
    }
}
