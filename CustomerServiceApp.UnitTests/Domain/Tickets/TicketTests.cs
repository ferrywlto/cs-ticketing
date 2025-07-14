using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.UnitTests.Domain.Tickets;

public class TicketTests
{
    private readonly Player _testPlayer;
    private readonly Agent _testAgent;

    public TicketTests()
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
            Avatar = "agent-avatar.jpg",
            PasswordHash = "agent_hashed_password"
        };
    }

    [Fact]
    public void Ticket_Should_Have_Required_Properties()
    {
        var title = "Can't load the game";
        var description = "Game crashes on startup";

        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            Creator = _testPlayer
        };

        Assert.Equal(title, ticket.Title);
        Assert.Equal(description, ticket.Description);
        Assert.Equal(_testPlayer, ticket.Creator);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.True(ticket.CreatedDate > DateTime.MinValue);
        Assert.Equal(ticket.CreatedDate, ticket.LastUpdateDate);
        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Empty(ticket.Messages);
    }

    [Fact]
    public void Ticket_Should_Start_With_Open_Status()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void AddReply_Should_Add_Message_To_Ticket()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        var originalLastUpdateDate = ticket.LastUpdateDate;
        
        // Add a small delay to ensure different timestamps
        Thread.Sleep(1);

        var reply = new Reply
        {
            Content = "This is a test reply",
            Author = _testPlayer
        };

        ticket.AddReply(reply);

        Assert.Single(ticket.Messages);
        Assert.Equal(reply, ticket.Messages.First());
        Assert.True(ticket.LastUpdateDate > originalLastUpdateDate);
    }

    [Fact]
    public void AddReply_By_Agent_Should_Change_Status_To_InResolution()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        var agentReply = new Reply
        {
            Content = "We're looking into this issue",
            Author = _testAgent
        };

        ticket.AddReply(agentReply);

        Assert.Equal(TicketStatus.InResolution, ticket.Status);
        Assert.Single(ticket.Messages);
    }

    [Fact]
    public void AddReply_By_Player_Should_Not_Change_Status()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        var playerReply = new Reply
        {
            Content = "Additional information",
            Author = _testPlayer
        };

        ticket.AddReply(playerReply);

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Single(ticket.Messages);
    }

    [Fact]
    public void Messages_Should_Be_Ordered_Chronologically()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        var firstReply = new Reply
        {
            Content = "First reply",
            Author = _testPlayer
        };

        var secondReply = new Reply
        {
            Content = "Second reply",
            Author = _testAgent
        };

        // Add with slight delay to ensure different timestamps
        ticket.AddReply(firstReply);
        Thread.Sleep(1); // Ensure different timestamps
        ticket.AddReply(secondReply);

        var messages = ticket.Messages;
        Assert.Equal(2, messages.Count);
        Assert.Equal(firstReply, messages[0]);
        Assert.Equal(secondReply, messages[1]);
    }

    [Fact]
    public void Resolve_Should_Change_Status_And_Set_ResolvedDate()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        // First move to InResolution by adding agent reply
        var agentReply = new Reply
        {
            Content = "Issue resolved",
            Author = _testAgent
        };
        ticket.AddReply(agentReply);

        var beforeResolve = DateTime.UtcNow;
        ticket.Resolve();

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.True(ticket.ResolvedDate.HasValue);
        Assert.True(ticket.ResolvedDate.Value >= beforeResolve);
        Assert.True(ticket.LastUpdateDate >= beforeResolve);
    }

    [Fact]
    public void Resolve_Should_Throw_When_Not_InResolution()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        Assert.Throws<InvalidOperationException>(() => ticket.Resolve());
    }

    [Fact]
    public void AddReply_Should_Throw_When_Ticket_Is_Resolved()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        // Move to InResolution and then resolve
        var agentReply = new Reply
        {
            Content = "Issue resolved",
            Author = _testAgent
        };
        ticket.AddReply(agentReply);
        ticket.Resolve();

        var newReply = new Reply
        {
            Content = "New reply after resolution",
            Author = _testPlayer
        };

        Assert.Throws<InvalidOperationException>(() => ticket.AddReply(newReply));
    }

    [Fact]
    public void AddReply_Should_Throw_When_Reply_Is_Null()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        Assert.Throws<ArgumentNullException>(() => ticket.AddReply(null!));
    }

    [Fact]
    public void Messages_Should_Be_ReadOnly()
    {
        var ticket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };

        var messages = ticket.Messages;
        Assert.IsAssignableFrom<IReadOnlyList<Reply>>(messages);
    }
}
