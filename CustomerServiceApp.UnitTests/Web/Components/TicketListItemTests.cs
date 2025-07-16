using Bunit;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Web.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CustomerServiceApp.UnitTests.Web.Components;

public class TicketListItemTests : TestContext
{
    [Fact]
    public void TicketListItem_ShouldRenderTicketInfo_WhenTicketProvided()
    {
        var ticket = new TicketDto(
            Guid.NewGuid(),
            "Test Ticket Title",
            "Test description",
            new PlayerDto(Guid.NewGuid(), "player@example.com", "John Player", "P001", "/img/player.jpg"),
            "Open",
            DateTime.Now.AddDays(-2),
            DateTime.Now.AddHours(-1),
            null,
            null,
            new List<ReplyDto>()
        );

        var component = RenderComponent<TicketListItem>(parameters => parameters
            .Add(p => p.Ticket, ticket)
            .Add(p => p.IsSelected, false)
            .Add(p => p.OnTicketSelected, () => { }));

        Assert.Contains("Test Ticket Title", component.Markup);
        Assert.Contains("John Player", component.Markup);
        Assert.Contains("P001", component.Markup);
        Assert.Contains("Open", component.Markup);
        Assert.Contains("/img/player.jpg", component.Markup);
    }

    [Fact]
    public void TicketListItem_ShouldShowSelectedState_WhenIsSelectedTrue()
    {
        var ticket = new TicketDto(
            Guid.NewGuid(),
            "Selected Ticket",
            "Description",
            new PlayerDto(Guid.NewGuid(), "player@example.com", "Player", "P001"),
            "Open",
            DateTime.Now,
            DateTime.Now,
            null,
            null,
            new List<ReplyDto>()
        );

        var component = RenderComponent<TicketListItem>(parameters => parameters
            .Add(p => p.Ticket, ticket)
            .Add(p => p.IsSelected, true)
            .Add(p => p.OnTicketSelected, () => { }));

        Assert.Contains("selected", component.Markup);
    }

    [Fact]
    public void TicketListItem_ShouldShowDefaultAvatar_WhenCreatorHasNoAvatar()
    {
        var ticket = new TicketDto(
            Guid.NewGuid(),
            "No Avatar Ticket",
            "Description",
            new PlayerDto(Guid.NewGuid(), "player@example.com", "Player", "P001"),
            "Open",
            DateTime.Now,
            DateTime.Now,
            null,
            null,
            new List<ReplyDto>()
        );

        var component = RenderComponent<TicketListItem>(parameters => parameters
            .Add(p => p.Ticket, ticket)
            .Add(p => p.IsSelected, false)
            .Add(p => p.OnTicketSelected, () => { }));

        Assert.Contains("bi-person-circle", component.Markup);
        Assert.Contains("text-primary", component.Markup);
    }

    [Fact]
    public void TicketListItem_ShouldFormatDatesCorrectly()
    {
        var createdDate = new DateTime(2024, 12, 15, 10, 30, 0);
        var updatedDate = new DateTime(2024, 12, 16, 14, 45, 0);
        
        var ticket = new TicketDto(
            Guid.NewGuid(),
            "Date Test Ticket",
            "Description",
            new PlayerDto(Guid.NewGuid(), "player@example.com", "Player", "P001"),
            "Open",
            createdDate,
            updatedDate,
            null,
            null,
            new List<ReplyDto>()
        );

        var component = RenderComponent<TicketListItem>(parameters => parameters
            .Add(p => p.Ticket, ticket)
            .Add(p => p.IsSelected, false)
            .Add(p => p.OnTicketSelected, () => { }));

        Assert.Contains("Dec 15, 2024", component.Markup);
        Assert.Contains("Dec 16, 2024 14:45", component.Markup);
    }

    [Fact]
    public void TicketListItem_ShouldTriggerCallback_WhenClicked()
    {
        var ticket = new TicketDto(
            Guid.NewGuid(),
            "Clickable Ticket",
            "Description",
            new PlayerDto(Guid.NewGuid(), "player@example.com", "Player", "P001"),
            "Open",
            DateTime.Now,
            DateTime.Now,
            null,
            null,
            new List<ReplyDto>()
        );

        var callbackTriggered = false;
        var component = RenderComponent<TicketListItem>(parameters => parameters
            .Add(p => p.Ticket, ticket)
            .Add(p => p.IsSelected, false)
            .Add(p => p.OnTicketSelected, () => { callbackTriggered = true; }));

        var ticketElement = component.Find(".ticket-list-item");
        ticketElement.Click();

        Assert.True(callbackTriggered);
    }
}
