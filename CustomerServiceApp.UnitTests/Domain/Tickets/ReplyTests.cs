using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;
using Xunit;

namespace CustomerServiceApp.UnitTests.Domain.Tickets;

public class ReplyTests
{
    private readonly Player _testPlayer;
    private readonly Agent _testAgent;

    public ReplyTests()
    {
        _testPlayer = new Player
        {
            Email = "player@example.com",
            Name = "John Doe",
            Avatar = "avatar.jpg",
            PlayerNumber = "P12345",
            PasswordHash = "hashed_password"
        };
        
        _testAgent = new Agent
        {
            Email = "agent@example.com",
            Name = "CS Agent",
            Avatar = "cs-avatar.jpg",
            PasswordHash = "hashed_password"
        };
    }

    [Fact]
    public void Reply_Should_Have_Required_Properties()
    {
        var content = "Thank you for contacting us.";

        var reply = new Reply
        {
            Content = content,
            Author = _testAgent
        };

        Assert.Equal(content, reply.Content);
        Assert.Equal(_testAgent, reply.Author);
        Assert.True(reply.CreatedDate > DateTime.MinValue);
        Assert.NotEqual(Guid.Empty, reply.Id);
    }

    [Fact]
    public void Reply_By_Player_Should_Have_Player_Author()
    {
        var reply = new Reply
        {
            Content = "Player response",
            Author = _testPlayer
        };

        Assert.Equal(_testPlayer, reply.Author);
        Assert.IsType<Player>(reply.Author);
    }

    [Fact]
    public void Reply_By_Agent_Should_Have_Agent_Author()
    {
        var reply = new Reply
        {
            Content = "Agent response",
            Author = _testAgent
        };

        Assert.Equal(_testAgent, reply.Author);
        Assert.IsType<Agent>(reply.Author);
    }
}
