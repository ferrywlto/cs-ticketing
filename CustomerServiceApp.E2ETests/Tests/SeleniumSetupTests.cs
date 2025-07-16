using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CustomerServiceApp.E2ETests.Tests;

/// <summary>
/// Simple smoke test to verify that Selenium setup is working correctly.
/// This test doesn't require the application to be running.
/// </summary>
public class SeleniumSetupTests : IDisposable
{
    private IWebDriver? _driver;

    [Fact]
    public void Selenium_ShouldStartChromeDriver()
    {
        // Arrange
        var options = new ChromeOptions();
        options.AddArguments("--headless");
        options.AddArguments("--no-sandbox");
        options.AddArguments("--disable-dev-shm-usage");

        // Act
        _driver = new ChromeDriver(options);

        // Assert
        Assert.NotNull(_driver);
        Assert.True(_driver.WindowHandles.Count > 0);
    }

    [Fact]
    public void Selenium_ShouldNavigateToGoogle()
    {
        // Arrange
        var options = new ChromeOptions();
        options.AddArguments("--headless");
        options.AddArguments("--no-sandbox");
        options.AddArguments("--disable-dev-shm-usage");
        
        _driver = new ChromeDriver(options);

        // Act
        _driver.Navigate().GoToUrl("https://www.google.com");

        // Assert
        Assert.Contains("Google", _driver.Title);
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
