using Xunit;
using System.Text.RegularExpressions;
using System.IO;

namespace CustomerServiceApp.IntegrationTests.Web.Pages;

public class HomePageIntegrationTests
{
    [Fact]
    public void HomeLayout_ShouldNotContainAppIdElement()
    {
        // Arrange
        var homeLayoutPath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", 
            "CustomerServiceApp.Web", 
            "Layout", 
            "HomeLayout.razor");
        
        // Act & Assert
        if (File.Exists(homeLayoutPath))
        {
            var content = File.ReadAllText(homeLayoutPath);
            
            // Check for id="app" or id='app' in HomeLayout
            var appIdPattern = @"id\s*=\s*['""]app['""]";
            var matches = Regex.Matches(content, appIdPattern, RegexOptions.IgnoreCase);
            
            Assert.Empty(matches);
        }
        else
        {
            // If HomeLayout doesn't exist, that's also acceptable - just ensure no app ID conflicts
            Assert.True(true, "HomeLayout.razor not found - no app ID conflicts possible");
        }
    }

    [Fact]
    public void HomeRazor_ShouldNotContainAppIdElement()
    {
        // Arrange
        var homePagePath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", 
            "CustomerServiceApp.Web", 
            "Pages", 
            "Home.razor");
        
        // Act & Assert
        if (File.Exists(homePagePath))
        {
            var content = File.ReadAllText(homePagePath);
            
            // Check for id="app" or id='app' in Home.razor
            var appIdPattern = @"id\s*=\s*['""]app['""]";
            var matches = Regex.Matches(content, appIdPattern, RegexOptions.IgnoreCase);
            
            Assert.Empty(matches);
        }
        else
        {
            Assert.Fail("Home.razor file not found at expected location");
        }
    }

    [Fact]
    public void IndexHtml_ShouldHaveExactlyOneAppIdElement()
    {
        // Arrange
        var indexHtmlPath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", 
            "CustomerServiceApp.Web", 
            "wwwroot", 
            "index.html");
        
        // Act & Assert
        if (File.Exists(indexHtmlPath))
        {
            var content = File.ReadAllText(indexHtmlPath);
            
            // Check for id="app" or id='app' in index.html
            var appIdPattern = @"id\s*=\s*['""]app['""]";
            var matches = Regex.Matches(content, appIdPattern, RegexOptions.IgnoreCase);
            
            Assert.Single(matches);
        }
        else
        {
            Assert.Fail("index.html file not found at expected location");
        }
    }

    [Fact]
    public void BlazorComponents_ShouldNotContainMultipleAppIds()
    {
        // Arrange
        var webProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", 
            "CustomerServiceApp.Web");
        
        if (!Directory.Exists(webProjectPath))
        {
            Assert.Fail($"Web project directory not found at {webProjectPath}");
            return;
        }
        
        var razorFiles = Directory.GetFiles(webProjectPath, "*.razor", SearchOption.AllDirectories);
        
        // Act & Assert
        foreach (var file in razorFiles)
        {
            var content = File.ReadAllText(file);
            var appIdPattern = @"id\s*=\s*['""]app['""]";
            var matches = Regex.Matches(content, appIdPattern, RegexOptions.IgnoreCase);
            
            // No Razor component should contain id="app"
            Assert.True(matches.Count == 0, 
                $"File {Path.GetFileName(file)} contains {matches.Count} 'id=\"app\"' elements. Only index.html should have this.");
        }
    }

    [Fact]
    public void EntireWebProject_ShouldHaveOnlyOneAppIdAcrossAllFiles()
    {
        // Arrange
        var webProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", 
            "CustomerServiceApp.Web");
        
        if (!Directory.Exists(webProjectPath))
        {
            Assert.Fail($"Web project directory not found at {webProjectPath}");
            return;
        }
        
        var allFiles = Directory.GetFiles(webProjectPath, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".razor") || f.EndsWith(".html") || f.EndsWith(".cshtml"))
            .ToArray();
        
        var totalAppIdCount = 0;
        var filesWithAppId = new List<string>();
        
        // Act
        foreach (var file in allFiles)
        {
            var content = File.ReadAllText(file);
            var appIdPattern = @"id\s*=\s*['""]app['""]";
            var matches = Regex.Matches(content, appIdPattern, RegexOptions.IgnoreCase);
            
            if (matches.Count > 0)
            {
                totalAppIdCount += matches.Count;
                filesWithAppId.Add($"{Path.GetFileName(file)} ({matches.Count})");
            }
        }
        
        // Assert
        Assert.Equal(1, totalAppIdCount);
        Assert.Single(filesWithAppId);
        Assert.Contains("index.html (1)", filesWithAppId);
    }

    [Fact]
    public void BlazorRootComponentAttachment_ShouldHaveUnambiguousTarget()
    {
        // Arrange
        var webProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", 
            "CustomerServiceApp.Web");
        
        if (!Directory.Exists(webProjectPath))
        {
            Assert.Fail($"Web project directory not found at {webProjectPath}");
            return;
        }
        
        // Check index.html exists and has single app element
        var indexHtmlPath = Path.Combine(webProjectPath, "wwwroot", "index.html");
        var allRazorFiles = Directory.GetFiles(webProjectPath, "*.razor", SearchOption.AllDirectories);
        
        // Act & Assert
        Assert.True(File.Exists(indexHtmlPath), "index.html must exist for Blazor WebAssembly");
        
        var indexContent = File.ReadAllText(indexHtmlPath);
        var appIdPattern = @"id\s*=\s*['""]app['""]";
        var indexMatches = Regex.Matches(indexContent, appIdPattern, RegexOptions.IgnoreCase);
        
        Assert.Single(indexMatches);
        
        // Ensure no Razor components compete for the same ID
        foreach (var razorFile in allRazorFiles)
        {
            var razorContent = File.ReadAllText(razorFile);
            var razorMatches = Regex.Matches(razorContent, appIdPattern, RegexOptions.IgnoreCase);
            
            Assert.Empty(razorMatches);
        }
    }
}
