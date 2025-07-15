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
        await AuthenticateAsPlayerAsync();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"), // Player1 ID from seed data
            Title = "Integration Test Ticket",
            Description = "This is a test ticket created during integration testing."
        };

        var response = await PostAsync("/api/tickets", createTicketDto);
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

        ClearAuthentication();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Title = "Test Ticket",
            Description = "Test Description"
        };

        var response = await PostAsync("/api/tickets", createTicketDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_AsAgent_ReturnsForbidden()
    {
        await AuthenticateAsAgentAsync();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Title = "Test Ticket",
            Description = "Test Description"
        };

        var response = await PostAsync("/api/tickets", createTicketDto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTicket_WithValidId_ReturnsOk()
    {
        var ticket = await CreateSampleTicketAsync();
        
        var response = await GetAsync($"/api/tickets/{ticket.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<TicketDto>(response);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.Title, result.Title);
        Assert.Equal(ticket.Description, result.Description);
    }

    [Fact]
    public async Task GetTicket_WithInvalidId_ReturnsNotFound()
    {

        await AuthenticateAsPlayerAsync();
        var invalidId = Guid.NewGuid();


        var response = await GetAsync($"/api/tickets/{invalidId}");


        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTicket_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var ticketId = Guid.NewGuid();


        var response = await GetAsync($"/api/tickets/{ticketId}");


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUnresolvedTickets_AsAgent_ReturnsOk()
    {

        await CreateSampleTicketAsync(); // Create a ticket first
        await AuthenticateAsAgentAsync();


        var response = await GetAsync("/api/tickets/unresolved");


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<IEnumerable<TicketDto>>(response);
        Assert.NotNull(result);
        Assert.True(result.Any());
    }

    [Fact]
    public async Task GetUnresolvedTickets_AsPlayer_ReturnsForbidden()
    {

        await AuthenticateAsPlayerAsync();


        var response = await GetAsync("/api/tickets/unresolved");


        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUnresolvedTickets_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();


        var response = await GetAsync("/api/tickets/unresolved");


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTicketsByPlayer_AsPlayer_ReturnsOk()
    {

        await CreateSampleTicketAsync(); // Create a ticket first
        var playerId = new Guid("11111111-1111-1111-1111-111111111111"); // Player1 ID


        var response = await GetAsync($"/api/tickets/player/{playerId}");


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<IEnumerable<TicketDto>>(response);
        Assert.NotNull(result);
        Assert.All(result, ticket => Assert.Equal(playerId, ticket.Creator.Id));
    }

    [Fact]
    public async Task GetTicketsByPlayer_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var playerId = Guid.NewGuid();


        var response = await GetAsync($"/api/tickets/player/{playerId}");


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithValidData_ReturnsOk()
    {

        var ticket = await CreateSampleTicketAsync();
        await AuthenticateAsAgentAsync(); // Switch to agent to reply

        var createReplyDto = new CreateReplyDto
        {
            TicketId = ticket.Id,
            AuthorId = new Guid("22222222-2222-2222-2222-222222222222"), // Agent ID from seed data
            Content = "This is a reply from the agent."
        };


        var response = await PostAsync($"/api/tickets/{ticket.Id}/replies", createReplyDto);


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithInvalidTicketId_ReturnsNotFound()
    {

        await AuthenticateAsPlayerAsync();
        var invalidTicketId = Guid.NewGuid();

        var createReplyDto = new CreateReplyDto
        {
            TicketId = invalidTicketId,
            AuthorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Content = "This reply should fail."
        };


        var response = await PostAsync($"/api/tickets/{invalidTicketId}/replies", createReplyDto);


        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var ticketId = Guid.NewGuid();

        var createReplyDto = new CreateReplyDto
        {
            TicketId = ticketId,
            AuthorId = Guid.NewGuid(),
            Content = "Unauthorized reply."
        };


        var response = await PostAsync($"/api/tickets/{ticketId}/replies", createReplyDto);


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_AsAgent_ReturnsOk()
    {

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


        var response = await PutAsync($"/api/tickets/{ticket.Id}/resolve", new { });


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_AsPlayer_ReturnsForbidden()
    {

        var ticket = await CreateSampleTicketAsync();

        // Act (Player is still authenticated from CreateSampleTicketAsync)
        var response = await PutAsync($"/api/tickets/{ticket.Id}/resolve", new { });


        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_WithInvalidTicketId_ReturnsNotFound()
    {

        await AuthenticateAsAgentAsync();
        var invalidTicketId = Guid.NewGuid();


        var response = await PutAsync($"/api/tickets/{invalidTicketId}/resolve", new { });


        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var ticketId = Guid.NewGuid();


        var response = await PutAsync($"/api/tickets/{ticketId}/resolve", new { });


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_WithInvalidData_ReturnsBadRequest()
    {

        await AuthenticateAsPlayerAsync();
        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"),
            Title = "", // Invalid - empty title
            Description = "Valid description"
        };


        var response = await PostAsync("/api/tickets", createTicketDto);


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
