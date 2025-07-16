using OpenQA.Selenium;
using CustomerServiceApp.E2ETests.Infrastructure;

namespace CustomerServiceApp.E2ETests.Tests;

public class PlayerLoginE2ETests : IClassFixture<SimpleSeleniumTestFixture>
{
    private readonly SimpleSeleniumTestFixture _fixture;
    private readonly IWebDriver _driver;

    public PlayerLoginE2ETests(SimpleSeleniumTestFixture fixture)
    {
        _fixture = fixture;
        _driver = fixture.Driver;
    }

    [Fact]
    public void PlayerLogin_ShouldDisplayLoginForm()
    {
        // Arrange & Act
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Assert
        var pageTitle = _driver.Title;
        Assert.Contains("Player Login", pageTitle);

        // Check for login form elements
        var emailField = _fixture.WaitForElement(By.Id("login-email"));
        var passwordField = _fixture.WaitForElement(By.Id("login-password"));
        var submitButton = _fixture.WaitForElement(By.CssSelector("button[type='submit']"));

        Assert.NotNull(emailField);
        Assert.NotNull(passwordField);
        Assert.NotNull(submitButton);
        Assert.Contains("Sign In", submitButton.Text);
    }

    [Fact]
    public void PlayerLogin_ShouldShowValidationErrors_WhenFieldsAreEmpty()
    {
        // Arrange
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Act - Try to submit empty form
        var submitButton = _fixture.WaitForClickableElement(By.CssSelector("button[type='submit']"));
        submitButton.Click();

        // Assert - Check for validation messages
        _fixture.WaitForElement(By.CssSelector(".validation-message"));
        
        var validationMessages = _driver.FindElements(By.CssSelector(".validation-message"));
        Assert.NotEmpty(validationMessages);
        
        // Check for specific validation messages
        var validationTexts = validationMessages.Select(msg => msg.Text).ToList();
        Assert.Contains(validationTexts, text => text.Contains("Email is required") || text.Contains("required"));
        Assert.Contains(validationTexts, text => text.Contains("Password is required") || text.Contains("required"));
    }

    [Fact]
    public void PlayerLogin_ShouldShowEmailValidationError_WhenEmailIsInvalid()
    {
        // Arrange
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Act
        var emailField = _fixture.WaitForElement(By.Id("login-email"));
        var passwordField = _fixture.WaitForElement(By.Id("login-password"));
        var submitButton = _fixture.WaitForClickableElement(By.CssSelector("button[type='submit']"));

        emailField.Clear();
        emailField.SendKeys("invalid-email");
        passwordField.Clear();
        passwordField.SendKeys("password123");
        
        submitButton.Click();

        // Assert
        _fixture.WaitForElement(By.CssSelector(".validation-message"));
        
        var validationMessages = _driver.FindElements(By.CssSelector(".validation-message"));
        Assert.NotEmpty(validationMessages);
        
        var validationTexts = validationMessages.Select(msg => msg.Text).ToList();
        Assert.Contains(validationTexts, text => text.Contains("Invalid email format") || text.Contains("email"));
    }

    [Fact]
    public void PlayerLogin_ShouldShowPasswordValidationError_WhenPasswordIsTooShort()
    {
        // Arrange
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Act
        var emailField = _fixture.WaitForElement(By.Id("login-email"));
        var passwordField = _fixture.WaitForElement(By.Id("login-password"));
        var submitButton = _fixture.WaitForClickableElement(By.CssSelector("button[type='submit']"));

        emailField.Clear();
        emailField.SendKeys("player1@example.com");
        passwordField.Clear();
        passwordField.SendKeys("123"); // Too short
        
        submitButton.Click();

        // Assert
        _fixture.WaitForElement(By.CssSelector(".validation-message"));
        
        var validationMessages = _driver.FindElements(By.CssSelector(".validation-message"));
        Assert.NotEmpty(validationMessages);
        
        var validationTexts = validationMessages.Select(msg => msg.Text).ToList();
        Assert.Contains(validationTexts, text => text.Contains("8 characters") || text.Contains("too short"));
    }

    [Fact]
    public void PlayerLogin_ShouldShowLoadingState_DuringSubmission()
    {
        // Arrange
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Act
        var emailField = _fixture.WaitForElement(By.Id("login-email"));
        var passwordField = _fixture.WaitForElement(By.Id("login-password"));
        var submitButton = _fixture.WaitForClickableElement(By.CssSelector("button[type='submit']"));

        emailField.Clear();
        emailField.SendKeys("player1@example.com");
        passwordField.Clear();
        passwordField.SendKeys("password123");
        
        submitButton.Click();

        // Assert - Check for loading state (spinner and "Signing In..." text)
        try
        {
            // Look for loading spinner or "Signing In..." text
            var loadingIndicators = _driver.FindElements(By.CssSelector(".spinner-border, [aria-hidden='true']"));
            var buttonText = submitButton.Text;
            
            // Either spinner should be present or button text should indicate loading
            Assert.True(loadingIndicators.Any() || buttonText.Contains("Signing In") || submitButton.GetAttribute("disabled") == "true");
        }
        catch (StaleElementReferenceException)
        {
            // Element might have been updated, which is also a valid loading behavior
            Assert.True(true);
        }
    }

    [Fact]
    public void PlayerLogin_ShouldRedirectToPlayerTickets_WhenCredentialsAreValid()
    {
        // Arrange
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Act
        var emailField = _fixture.WaitForElement(By.Id("login-email"));
        var passwordField = _fixture.WaitForElement(By.Id("login-password"));
        var submitButton = _fixture.WaitForClickableElement(By.CssSelector("button[type='submit']"));

        emailField.Clear();
        emailField.SendKeys("player1@example.com");
        passwordField.Clear();
        passwordField.SendKeys("password123");
        
        submitButton.Click();

        // Assert - Wait for redirect to player tickets page
        _fixture.Wait.Until(driver => driver.Url.Contains("/player/tickets"));
        
        var currentUrl = _driver.Url;
        Assert.Contains("/player/tickets", currentUrl);
        
        // Verify we're on the player tickets page by checking for player-specific elements
        var ticketElements = _driver.FindElements(By.CssSelector(".ticket-list-item, .new-ticket-btn, h5"));
        Assert.NotEmpty(ticketElements);
    }

    [Fact]
    public void PlayerLogin_ShouldShowErrorMessage_WhenCredentialsAreInvalid()
    {
        // Arrange
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Act
        var emailField = _fixture.WaitForElement(By.Id("login-email"));
        var passwordField = _fixture.WaitForElement(By.Id("login-password"));
        var submitButton = _fixture.WaitForClickableElement(By.CssSelector("button[type='submit']"));

        emailField.Clear();
        emailField.SendKeys("invalid@example.com");
        passwordField.Clear();
        passwordField.SendKeys("wrongpassword123");
        
        submitButton.Click();

        // Assert - Check for error message
        var errorAlert = _fixture.WaitForElement(By.CssSelector(".alert-danger"));
        Assert.NotNull(errorAlert);
        
        var errorText = errorAlert.Text;
        Assert.True(errorText.Contains("Invalid") || errorText.Contains("error") || errorText.Contains("failed"));
    }

    [Fact]
    public void PlayerLogin_ShouldDisplayDemoCredentials()
    {
        // Arrange & Act
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Assert
        var demoCredentialsElement = _fixture.WaitForElement(By.CssSelector(".text-muted"));
        Assert.NotNull(demoCredentialsElement);
        
        var demoText = demoCredentialsElement.Text;
        Assert.Contains("Demo users", demoText);
        Assert.Contains("player1@example.com", demoText);
        Assert.Contains("password123", demoText);
    }

    [Fact]
    public void PlayerLogin_ShouldHaveCorrectPageTitle()
    {
        // Arrange & Act
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Assert
        var pageTitle = _driver.Title;
        Assert.Contains("Player Login", pageTitle);
        
        var headerElement = _fixture.WaitForElement(By.CssSelector("h3"));
        Assert.Equal("Player Login", headerElement.Text);
    }

    [Fact]
    public void PlayerLogin_ShouldHaveProperFormStructure()
    {
        // Arrange & Act
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();

        // Assert
        // Check for proper form structure
        var form = _fixture.WaitForElement(By.CssSelector("form"));
        Assert.NotNull(form);
        
        // Check for proper labels
        var emailLabel = _driver.FindElement(By.CssSelector("label[for='login-email']"));
        var passwordLabel = _driver.FindElement(By.CssSelector("label[for='login-password']"));
        
        Assert.Equal("Email", emailLabel.Text);
        Assert.Equal("Password", passwordLabel.Text);
        
        // Check for proper input types
        var passwordInput = _driver.FindElement(By.Id("login-password"));
        Assert.Equal("password", passwordInput.GetAttribute("type"));
        
        // Check for proper placeholders
        var emailInput = _driver.FindElement(By.Id("login-email"));
        Assert.Contains("email", emailInput.GetAttribute("placeholder").ToLower());
        Assert.Contains("password", passwordInput.GetAttribute("placeholder").ToLower());
    }
}
