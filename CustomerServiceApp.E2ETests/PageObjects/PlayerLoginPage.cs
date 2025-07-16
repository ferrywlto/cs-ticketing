using OpenQA.Selenium;
using CustomerServiceApp.E2ETests.Infrastructure;

namespace CustomerServiceApp.E2ETests.PageObjects;

public class PlayerLoginPage
{
    private readonly SimpleSeleniumTestFixture _fixture;
    private readonly IWebDriver _driver;

    public PlayerLoginPage(SimpleSeleniumTestFixture fixture)
    {
        _fixture = fixture;
        _driver = fixture.Driver;
    }

    // Page Elements
    public IWebElement EmailField => _fixture.WaitForElement(By.Id("login-email"));
    public IWebElement PasswordField => _fixture.WaitForElement(By.Id("login-password"));
    public IWebElement SubmitButton => _fixture.WaitForClickableElement(By.CssSelector("button[type='submit']"));
    public IWebElement PageHeader => _fixture.WaitForElement(By.CssSelector("h3"));
    public IWebElement DemoCredentials => _fixture.WaitForElement(By.CssSelector(".text-muted"));

    // Optional Elements (may not always be present)
    public IWebElement? ErrorAlert
    {
        get
        {
            try
            {
                return _driver.FindElement(By.CssSelector(".alert-danger"));
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }
    }

    public IList<IWebElement> ValidationMessages => _driver.FindElements(By.CssSelector(".validation-message"));

    // Page Actions
    public void NavigateToPage()
    {
        _fixture.NavigateToPlayerLogin();
        _fixture.WaitForPageLoad();
    }

    public void EnterEmail(string email)
    {
        EmailField.Clear();
        EmailField.SendKeys(email);
    }

    public void EnterPassword(string password)
    {
        PasswordField.Clear();
        PasswordField.SendKeys(password);
    }

    public void ClickSubmit()
    {
        SubmitButton.Click();
    }

    public void LoginWithCredentials(string email, string password)
    {
        EnterEmail(email);
        EnterPassword(password);
        ClickSubmit();
    }

    public void LoginWithValidPlayerCredentials()
    {
        LoginWithCredentials("player1@example.com", "password123");
    }

    public void LoginWithInvalidCredentials()
    {
        LoginWithCredentials("invalid@example.com", "wrongpassword123");
    }

    // Page Validations
    public bool IsOnLoginPage()
    {
        return _driver.Url.Contains("/player/login") && PageHeader.Text == "Player Login";
    }

    public bool HasValidationErrors()
    {
        return ValidationMessages.Any();
    }

    public bool HasErrorAlert()
    {
        return ErrorAlert != null;
    }

    public bool IsSubmitButtonDisabled()
    {
        return SubmitButton.GetAttribute("disabled") == "true";
    }

    public bool IsLoadingStateVisible()
    {
        try
        {
            var spinner = _driver.FindElement(By.CssSelector(".spinner-border"));
            return spinner.Displayed;
        }
        catch (NoSuchElementException)
        {
            return SubmitButton.Text.Contains("Signing In");
        }
    }

    public string GetPageTitle()
    {
        return _driver.Title;
    }

    public string GetHeaderText()
    {
        return PageHeader.Text;
    }

    public string GetDemoCredentialsText()
    {
        return DemoCredentials.Text;
    }

    public List<string> GetValidationMessages()
    {
        return ValidationMessages.Select(msg => msg.Text).ToList();
    }

    public string? GetErrorMessage()
    {
        return ErrorAlert?.Text;
    }

    // Wait Methods
    public void WaitForRedirectToPlayerTickets()
    {
        _fixture.Wait.Until(driver => driver.Url.Contains("/player/tickets"));
    }

    public void WaitForValidationErrors()
    {
        _fixture.WaitForElement(By.CssSelector(".validation-message"));
    }

    public void WaitForErrorAlert()
    {
        _fixture.WaitForElement(By.CssSelector(".alert-danger"));
    }
}
