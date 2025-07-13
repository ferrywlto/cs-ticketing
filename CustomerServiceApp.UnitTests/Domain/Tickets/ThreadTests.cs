using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;
using Xunit;

namespace CustomerServiceApp.UnitTests.Domain.Tickets;

public class TicketThreadTests
{
    private readonly Player _testPlayer;
    private readonly Agent _testAgent;
    private readonly Ticket _testTicket;

    public TicketThreadTests()
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
        
        _testTicket = new Ticket
        {
            Title = "Test Title",
            Description = "Test Description",
            Creator = _testPlayer
        };
    }

    [Fact]
    public void TicketThread_Should_Have_Required_Properties()
    {
        var thread = new TicketThread
        {
            Ticket = _testTicket
        };

        Assert.Equal(_testTicket, thread.Ticket);
        Assert.Equal(TicketStatus.Open, thread.Status);
        Assert.True(thread.CreatedDate > DateTime.MinValue);
        Assert.True(thread.LastUpdateDate > DateTime.MinValue);
        Assert.NotEqual(Guid.Empty, thread.Id);
        Assert.Empty(thread.Messages);
    }

    [Fact]
    public void TicketThread_Should_Add_Reply_And_Update_LastUpdateDate()
    {
        var thread = new TicketThread
        {
            Ticket = _testTicket
        };
        var originalLastUpdate = thread.LastUpdateDate;

        // Small delay to ensure different timestamps
        System.Threading.Thread.Sleep(1);

        var reply = new Reply
        {
            Content = "Thank you for contacting us.",
            Author = _testAgent,
            Ticket = _testTicket
        };

        thread.AddReply(reply);

        Assert.Single(thread.Messages);
        Assert.Contains(reply, thread.Messages);
        Assert.True(thread.LastUpdateDate > originalLastUpdate);
    }

    [Fact]
    public void TicketThread_Should_Change_Status_To_InResolution_When_Agent_Replies()
    {
        var thread = new TicketThread
        {
            Ticket = _testTicket
        };

        var agentReply = new Reply
        {
            Content = "We are investigating this issue.",
            Author = _testAgent,
            Ticket = _testTicket
        };

        thread.AddReply(agentReply);

        Assert.Equal(TicketStatus.InResolution, thread.Status);
        Assert.Equal(TicketStatus.InResolution, thread.Ticket.Status);
    }

    [Fact]
    public void TicketThread_Should_Not_Change_Status_When_Player_Replies()
    {
        var thread = new TicketThread
        {
            Ticket = _testTicket
        };

        var playerReply = new Reply
        {
            Content = "Additional information from player.",
            Author = _testPlayer,
            Ticket = _testTicket
        };

        thread.AddReply(playerReply);

        Assert.Equal(TicketStatus.Open, thread.Status);
        Assert.Equal(TicketStatus.Open, thread.Ticket.Status);
    }

    [Fact]
    public void TicketThread_Should_Resolve_Ticket()
    {
        var thread = new TicketThread
        {
            Ticket = _testTicket
        };

        // First, move to InResolution
        var agentReply = new Reply
        {
            Content = "We are working on this.",
            Author = _testAgent,
            Ticket = _testTicket
        };
        thread.AddReply(agentReply);

        var originalLastUpdate = thread.LastUpdateDate;
        System.Threading.Thread.Sleep(1);

        thread.ResolveTicket();

        Assert.Equal(TicketStatus.Resolved, thread.Status);
        Assert.Equal(TicketStatus.Resolved, thread.Ticket.Status);
        Assert.True(thread.LastUpdateDate > originalLastUpdate);
        Assert.True(thread.Ticket.ResolvedDate.HasValue);
    }

    [Fact]
    public void TicketThread_Messages_Should_Be_Sorted_By_CreatedDate()
    {
        var thread = new TicketThread
        {
            Ticket = _testTicket
        };

        var firstReply = new Reply
        {
            Content = "First reply",
            Author = _testPlayer,
            Ticket = _testTicket
        };

        System.Threading.Thread.Sleep(1);

        var secondReply = new Reply
        {
            Content = "Second reply",
            Author = _testAgent,
            Ticket = _testTicket
        };

        thread.AddReply(secondReply);
        thread.AddReply(firstReply);

        var messages = thread.Messages.ToList();
        Assert.Equal(2, messages.Count);
        Assert.True(messages[0].CreatedDate <= messages[1].CreatedDate);
    }

    [Fact]
    public void TicketThread_Cannot_Be_Resolved_When_Open()
    {
        var thread = new TicketThread
        {
            Ticket = _testTicket
        };

        Assert.Throws<InvalidOperationException>(() => thread.ResolveTicket());
    }
}
