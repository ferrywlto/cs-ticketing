using Bunit;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Web.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CustomerServiceApp.UnitTests.Web.Components;

public class MessageCardTests : TestContext
{
    [Fact]
    public void MessageCard_ShouldRenderPlayerMessage_WhenAuthorIsPlayer()
    {
        var message = new ReplyDto(
            Guid.NewGuid(),
            "Test message content",
            new PlayerDto(Guid.NewGuid(), "player1@example.com", "John Player", "P001", "/img/player.jpg"),
            DateTime.UtcNow
        );

        var component = RenderComponent<MessageCard>(parameters => parameters
            .Add(p => p.Message, message)
            .Add(p => p.IsFromAgent, false));

        Assert.Contains("Test message content", component.Markup);
        Assert.Contains("border-primary", component.Markup);
        Assert.Contains("John Player", component.Markup);
        Assert.Contains("/img/player.jpg", component.Markup);
    }

    [Fact]
    public void MessageCard_ShouldRenderAgentMessage_WhenAuthorIsAgent()
    {
        var message = new ReplyDto(
            Guid.NewGuid(),
            "Agent response content",
            new AgentDto(Guid.NewGuid(), "agent@example.com", "CS Agent", "/img/agent.jpg"),
            DateTime.UtcNow
        );

        var component = RenderComponent<MessageCard>(parameters => parameters
            .Add(p => p.Message, message)
            .Add(p => p.IsFromAgent, true));

        Assert.Contains("Agent response content", component.Markup);
        Assert.Contains("border-success", component.Markup);
        Assert.Contains("CS Agent", component.Markup);
        Assert.Contains("/img/agent.jpg", component.Markup);
    }

    [Fact]
    public void MessageCard_ShouldShowDefaultAvatar_WhenAuthorHasNoAvatar()
    {
        var message = new ReplyDto(
            Guid.NewGuid(),
            "Message without avatar",
            new PlayerDto(Guid.NewGuid(), "user@example.com", "User", "P001"),
            DateTime.UtcNow
        );

        var component = RenderComponent<MessageCard>(parameters => parameters
            .Add(p => p.Message, message)
            .Add(p => p.IsFromAgent, false));

        Assert.Contains("bi-person-circle", component.Markup);
        Assert.Contains("text-primary", component.Markup);
    }

    [Fact]
    public void MessageCard_ShouldShowAgentIcon_WhenAgentHasNoAvatar()
    {
        var message = new ReplyDto(
            Guid.NewGuid(),
            "Agent message without avatar",
            new AgentDto(Guid.NewGuid(), "agent@example.com", "CS Agent"),
            DateTime.UtcNow
        );

        var component = RenderComponent<MessageCard>(parameters => parameters
            .Add(p => p.Message, message)
            .Add(p => p.IsFromAgent, true));

        Assert.Contains("bi-shield-fill", component.Markup);
        Assert.Contains("text-success", component.Markup);
    }

    [Fact]
    public void MessageCard_ShouldFormatDateCorrectly()
    {
        var testDate = new DateTime(2024, 12, 15, 14, 30, 0);
        var message = new ReplyDto(
            Guid.NewGuid(),
            "Test message",
            new PlayerDto(Guid.NewGuid(), "test@example.com", "Test User", "P001"),
            testDate
        );

        var component = RenderComponent<MessageCard>(parameters => parameters
            .Add(p => p.Message, message)
            .Add(p => p.IsFromAgent, false));

        Assert.Contains("Dec 15, 2024 14:30", component.Markup);
    }
}
