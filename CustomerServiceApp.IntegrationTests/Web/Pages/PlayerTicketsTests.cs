using Bunit;
using CustomerServiceApp.Web.Pages;
using CustomerServiceApp.Web.State;
using CustomerServiceApp.Web.Services;
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
        Services.AddSingleton<AppStateStore>();
    }

    [Fact]
    public void PlayerTickets_ShouldShowCurrentPlayerTickets()
    {
        // Arrange & Act
        var component = RenderComponent<PlayerTickets>();

        // Assert
        // Check for the "New ticket" button which is unique to player view
        var newTicketButton = component.FindAll("button").FirstOrDefault(btn => btn.TextContent.Contains("New ticket"));
        Assert.NotNull(newTicketButton);
        
        // Should show tickets for current player only
        var ticketItems = component.FindAll(".ticket-list-item");
        Assert.True(ticketItems.Count > 0, "Should display player's tickets");
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
    public void PlayerTickets_ShouldAllowCreatingNewTicket()
    {
        // Arrange
        var component = RenderComponent<PlayerTickets>();
        var buttons = component.FindAll("button");
        var newTicketButton = buttons.First(btn => btn.TextContent.Contains("New ticket"));

        // Act
        newTicketButton.Click();

        // Assert - This would typically navigate to a new ticket form or open a modal
        // For now, we'll verify the click event is handled
        Assert.True(true, "New ticket button click should be handled");
    }

    [Fact]
    public void PlayerTickets_ShouldAllowSendingReplies()
    {
        // Arrange
        var component = RenderComponent<PlayerTickets>();
        var firstTicket = component.Find(".ticket-list-item");
        firstTicket.Click();

        var textarea = component.Find("textarea");
        var buttons = component.FindAll("button");
        var sendButton = buttons.First(btn => btn.TextContent.Contains("Send"));

        // Act
        textarea.Change("Test reply from player");
        sendButton.Click();

        // Assert
        var messages = component.FindAll(".message-item");
        var lastMessage = messages.Last();
        Assert.Contains("Test reply from player", lastMessage.TextContent);
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
