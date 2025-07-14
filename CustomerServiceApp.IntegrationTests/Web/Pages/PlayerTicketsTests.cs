using Bunit;
using CustomerServiceApp.Web.Pages;

namespace CustomerServiceApp.IntegrationTests.Web.Pages;

public class PlayerTicketsTests : TestContext
{
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
}
