using System.Net;
using CustomerServiceApp.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerServiceApp.IntegrationTests.API.Controllers;

public class TicketsControllerIntegrationTests : ApiIntegrationTestBase
{
    public TicketsControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTicket_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsPlayerAsync();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"), // Player1 ID from seed data
            Title = "Integration Test Ticket",
            Description = "This is a test ticket created during integration testing."
        };

        // Act
        var response = await PostAsync("/api/tickets", createTicketDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await DeserializeAsync<TicketDto>(response);
        Assert.Equal(createTicketDto.Title, result.Title);
        Assert.Equal(createTicketDto.Description, result.Description);
        Assert.Equal(createTicketDto.CreatorId, result.Creator.Id);
        Assert.Equal("Open", result.Status);
    }

    [Fact]
    public async Task CreateTicket_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthentication();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Title = "Test Ticket",
            Description = "Test Description"
        };

        // Act
        var response = await PostAsync("/api/tickets", createTicketDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_AsAgent_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsAgentAsync();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Title = "Test Ticket",
            Description = "Test Description"
        };

        // Act
        var response = await PostAsync("/api/tickets", createTicketDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTicket_WithValidId_ReturnsOk()
    {
        // Arrange
        var ticket = await CreateSampleTicketAsync();
        
        // Act
        var response = await GetAsync($"/api/tickets/{ticket.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<TicketDto>(response);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.Title, result.Title);
        Assert.Equal(ticket.Description, result.Description);
    }

    [Fact]
    public async Task GetTicket_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsPlayerAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/tickets/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTicket_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthentication();
        var ticketId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/tickets/{ticketId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUnresolvedTickets_AsAgent_ReturnsOk()
    {
        // Arrange
        await CreateSampleTicketAsync(); // Create a ticket first
        await AuthenticateAsAgentAsync();

        // Act
        var response = await GetAsync("/api/tickets/unresolved");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<IEnumerable<TicketDto>>(response);
        Assert.NotNull(result);
        Assert.True(result.Any());
    }

    [Fact]
    public async Task GetUnresolvedTickets_AsPlayer_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsPlayerAsync();

        // Act
        var response = await GetAsync("/api/tickets/unresolved");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUnresolvedTickets_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await GetAsync("/api/tickets/unresolved");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTicketsByPlayer_AsPlayer_ReturnsOk()
    {
        // Arrange
        await CreateSampleTicketAsync(); // Create a ticket first
        var playerId = new Guid("11111111-1111-1111-1111-111111111111"); // Player1 ID

        // Act
        var response = await GetAsync($"/api/tickets/player/{playerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<IEnumerable<TicketDto>>(response);
        Assert.NotNull(result);
        Assert.All(result, ticket => Assert.Equal(playerId, ticket.Creator.Id));
    }

    [Fact]
    public async Task GetTicketsByPlayer_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthentication();
        var playerId = Guid.NewGuid();

        // Act
        var response = await GetAsync($"/api/tickets/player/{playerId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithValidData_ReturnsOk()
    {
        // Arrange
        var ticket = await CreateSampleTicketAsync();
        await AuthenticateAsAgentAsync(); // Switch to agent to reply

        var createReplyDto = new CreateReplyDto
        {
            TicketId = ticket.Id,
            AuthorId = new Guid("22222222-2222-2222-2222-222222222222"), // Agent ID from seed data
            Content = "This is a reply from the agent."
        };

        // Act
        var response = await PostAsync($"/api/tickets/{ticket.Id}/replies", createReplyDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithInvalidTicketId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsPlayerAsync();
        var invalidTicketId = Guid.NewGuid();

        var createReplyDto = new CreateReplyDto
        {
            TicketId = invalidTicketId,
            AuthorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Content = "This reply should fail."
        };

        // Act
        var response = await PostAsync($"/api/tickets/{invalidTicketId}/replies", createReplyDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthentication();
        var ticketId = Guid.NewGuid();

        var createReplyDto = new CreateReplyDto
        {
            TicketId = ticketId,
            AuthorId = Guid.NewGuid(),
            Content = "Unauthorized reply."
        };

        // Act
        var response = await PostAsync($"/api/tickets/{ticketId}/replies", createReplyDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_AsAgent_ReturnsOk()
    {
        // Arrange
        var ticket = await CreateSampleTicketAsync();
        
        // First, add an agent reply to change status to "InResolution"
        await AuthenticateAsAgentAsync();
        var createReplyDto = new CreateReplyDto
        {
            TicketId = ticket.Id,
            AuthorId = new Guid("22222222-2222-2222-2222-222222222222"), // Agent ID
            Content = "Agent reply to move to InResolution status."
        };
        await PostAsync($"/api/tickets/{ticket.Id}/replies", createReplyDto);

        // Act
        var response = await PutAsync($"/api/tickets/{ticket.Id}/resolve", new { });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_AsPlayer_ReturnsForbidden()
    {
        // Arrange
        var ticket = await CreateSampleTicketAsync();

        // Act (Player is still authenticated from CreateSampleTicketAsync)
        var response = await PutAsync($"/api/tickets/{ticket.Id}/resolve", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_WithInvalidTicketId_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsAgentAsync();
        var invalidTicketId = Guid.NewGuid();

        // Act
        var response = await PutAsync($"/api/tickets/{invalidTicketId}/resolve", new { });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        ClearAuthentication();
        var ticketId = Guid.NewGuid();

        // Act
        var response = await PutAsync($"/api/tickets/{ticketId}/resolve", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsPlayerAsync();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Title = "", // Invalid - empty title
            Description = "Valid description"
        };

        // Act
        var response = await PostAsync("/api/tickets", createTicketDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
