using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CustomerServiceApp.Web.Pages;

namespace CustomerServiceApp.IntegrationTests.Web.Pages;

public class AgentTicketsTests : TestContext
{
    [Fact]
    public void AgentTickets_ShouldShowAllUnresolvedTickets()
    {
        // Arrange & Act
        var component = RenderComponent<AgentTickets>();

        // Assert
        // Check for the "Unresolved Tickets" header which is unique to agent view
        var headerElement = component.FindAll("h5").FirstOrDefault(h => h.TextContent.Contains("Unresolved Tickets"));
        Assert.NotNull(headerElement);
        
        // Should show tickets from all players with unresolved status
        var ticketItems = component.FindAll(".ticket-list-item");
        Assert.True(ticketItems.Count > 0, "Should display unresolved tickets");
        
        // Verify no resolved tickets are shown
        var ticketTexts = component.FindAll(".ticket-list-item").Select(item => item.TextContent);
        Assert.DoesNotContain(ticketTexts, text => text.Contains("Resolved"));
    }

    [Fact]
    public void AgentTickets_ShouldHideNewTicketButton()
    {
        // Arrange & Act
        var component = RenderComponent<AgentTickets>();

        // Assert
        var buttons = component.FindAll("button");
        var newTicketButtons = buttons.Where(btn => btn.TextContent.Contains("New ticket"));
        Assert.Empty(newTicketButtons);
    }

    [Fact]
    public void AgentTickets_ShouldNotHaveNavigationMenu()
    {
        // Arrange & Act
        var component = RenderComponent<AgentTickets>();

        // Assert
        var navMenus = component.FindAll(".sidebar, .nav-menu, .navbar");
        Assert.Empty(navMenus);
    }

    [Fact]
    public void AgentTickets_ShouldNotHaveTopHeader()
    {
        // Arrange & Act
        var component = RenderComponent<AgentTickets>();

        // Assert
        var topRows = component.FindAll(".top-row");
        Assert.Empty(topRows);
    }

    [Fact]
    public void AgentTickets_ShouldAllowTicketSelection()
    {
        // Arrange
        var component = RenderComponent<AgentTickets>();
        var firstTicket = component.Find(".ticket-list-item");

        // Act
        firstTicket.Click();

        // Assert
        var classes = firstTicket.GetAttribute("class");
        Assert.Contains("selected", classes);
    }

    [Fact]
    public void AgentTickets_ShouldAllowSendingReplies()
    {
        // Arrange
        var component = RenderComponent<AgentTickets>();
        var firstTicket = component.Find(".ticket-list-item");
        firstTicket.Click();

        var textarea = component.Find("textarea");
        var buttons = component.FindAll("button");
        var sendButton = buttons.First(btn => btn.TextContent.Contains("Send"));

        // Act
        textarea.Change("Test reply from agent");
        sendButton.Click();

        // Assert
        var messages = component.FindAll(".message-item");
        var lastMessage = messages.Last();
        Assert.Contains("Test reply from agent", lastMessage.TextContent);
        Assert.Contains("CS Agent", lastMessage.TextContent);
    }
}
