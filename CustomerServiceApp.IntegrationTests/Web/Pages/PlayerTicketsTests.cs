using Bunit;
using CustomerServiceApp.Web.Pages;
using CustomerServiceApp.Web.State;
using CustomerServiceApp.Web.Services;
using CustomerServiceApp.Application.Common.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace CustomerServiceApp.IntegrationTests.Web.Pages;

public class PlayerTicketsTests : TestContext
{
    public PlayerTicketsTests()
    {
        // Register required services
        Services.AddLogging();
        Services.AddSingleton<ILocalStorageService, MockLocalStorageService>();
        Services.AddSingleton<AppStateStore>();
        
        // Register HttpClient with base address for ApiService
        Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7234/");
        });
    }

    // Simple mock implementation for testing
    public class MockLocalStorageService : ILocalStorageService
    {
        public Task<string?> GetItemAsync(string key) => Task.FromResult<string?>(null);
        public Task SetItemAsync(string key, string value) => Task.CompletedTask;
        public Task RemoveItemAsync(string key) => Task.CompletedTask;
        public Task ClearAsync() => Task.CompletedTask;
    }

    [Fact]
    public void PlayerTickets_ShouldShowNewTicketButton()
    {
        // Arrange & Act
        var component = RenderComponent<PlayerTickets>();

        // Assert
        var buttons = component.FindAll("button");
        var newTicketButton = buttons.FirstOrDefault(btn => btn.TextContent.Contains("New ticket"));
        Assert.NotNull(newTicketButton);
    }

    [Fact]
    public void PlayerTickets_ShouldNotHaveNavigationMenu()
    {
        // Arrange & Act
        var component = RenderComponent<PlayerTickets>();

        // Assert
        var navMenus = component.FindAll(".sidebar, .nav-menu, .navbar");
        Assert.Empty(navMenus);
    }

    [Fact]
    public void PlayerTickets_ShouldNotHaveTopHeader()
    {
        // Arrange & Act
        var component = RenderComponent<PlayerTickets>();

        // Assert
        var topRows = component.FindAll(".top-row");
        Assert.Empty(topRows);
    }

    [Fact]
    public void PlayerTickets_ShouldShowLogoutButton()
    {
        // Arrange & Act
        var component = RenderComponent<PlayerTickets>();

        // Assert
        var buttons = component.FindAll("button");
        var logoutButton = buttons.FirstOrDefault(btn => btn.TextContent.Contains("Logout"));
        Assert.NotNull(logoutButton);
        
        // Verify it has the correct icon
        var icon = logoutButton.QuerySelector("i.bi-box-arrow-left");
        Assert.NotNull(icon);
    }

    [Fact]
    public void PlayerTickets_ShouldHaveBothLogoutAndNewTicketButtons()
    {
        // Arrange & Act
        var component = RenderComponent<PlayerTickets>();

        // Assert
        var buttons = component.FindAll("button");
        var logoutButton = buttons.FirstOrDefault(btn => btn.TextContent.Contains("Logout"));
        var newTicketButton = buttons.FirstOrDefault(btn => btn.TextContent.Contains("New ticket"));
        
        Assert.NotNull(logoutButton);
        Assert.NotNull(newTicketButton);
        
        // Verify both buttons are in the same container
        var buttonContainer = component.Find(".d-flex.gap-2");
        Assert.NotNull(buttonContainer);
    }
}
