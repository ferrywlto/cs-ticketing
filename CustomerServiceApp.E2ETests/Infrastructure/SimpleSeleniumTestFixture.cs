using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace CustomerServiceApp.E2ETests.Infrastructure;

public class SimpleSeleniumTestFixture : IDisposable
{
    public IWebDriver Driver { get; private set; } = null!;
    public WebDriverWait Wait { get; private set; } = null!;
    public string BaseUrl { get; private set; } = "https://localhost:7145"; // Web app URL

    public SimpleSeleniumTestFixture()
    {
        InitializeWebDriver();
    }

    private void InitializeWebDriver()
    {
        var options = new ChromeOptions();
        // Comment out headless for debugging, uncomment for CI/CD
        // options.AddArguments("--headless");
        options.AddArguments("--no-sandbox");
        options.AddArguments("--disable-dev-shm-usage");
        options.AddArguments("--disable-gpu");
        options.AddArguments("--window-size=1920,1080");
        options.AddArguments("--ignore-certificate-errors");
        options.AddArguments("--ignore-ssl-errors");
        options.AddArguments("--allow-running-insecure-content");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
    }

    public void NavigateToPlayerLogin()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/player/login");
    }

    public void NavigateToAgentLogin()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/agent/login");
    }

    public void NavigateToPlayerTickets()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/player/tickets");
    }

    public void NavigateToAgentTickets()
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/agent/tickets");
    }

    public IWebElement WaitForElement(By locator, int timeoutSeconds = 30)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(ExpectedConditions.ElementIsVisible(locator));
    }

    public IWebElement WaitForClickableElement(By locator, int timeoutSeconds = 30)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    public bool IsElementPresent(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public void WaitForPageLoad()
    {
        Wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
    }

    public void Dispose()
    {
        Driver?.Quit();
        Driver?.Dispose();
    }
}
