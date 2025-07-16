using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using CustomerServiceApp.API;
using System.Diagnostics;

namespace CustomerServiceApp.E2ETests.Infrastructure;

public class SeleniumTestFixture : IDisposable
{
    private Process? _apiProcess;
    private Process? _webProcess;
    
    public IWebDriver Driver { get; private set; } = null!;
    public WebDriverWait Wait { get; private set; } = null!;
    public string BaseUrl { get; private set; } = "https://localhost:7145"; // Web app URL
    public string ApiBaseUrl { get; private set; } = "https://localhost:7234"; // API URL

    public SeleniumTestFixture()
    {
        StartApplications();
        InitializeWebDriver();
    }

    private void StartApplications()
    {
        // Start API project
        _apiProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project ../../../CustomerServiceApp.API --launch-profile https",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            }
        };
        
        // Start Web project
        _webProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project ../../../CustomerServiceApp.Web --launch-profile https",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            }
        };

        _apiProcess.Start();
        _webProcess.Start();
        
        // Wait for applications to start
        Thread.Sleep(10000); // Give apps time to start
    }

    private void InitializeWebDriver()
    {
        var options = new ChromeOptions();
        options.AddArguments("--headless"); // Run in headless mode for CI/CD
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

    public void Dispose()
    {
        Driver?.Quit();
        Driver?.Dispose();
        
        // Kill the processes
        try
        {
            _apiProcess?.Kill();
            _apiProcess?.Dispose();
        }
        catch { /* Ignore cleanup errors */ }
        
        try
        {
            _webProcess?.Kill();
            _webProcess?.Dispose();
        }
        catch { /* Ignore cleanup errors */ }
    }
}
