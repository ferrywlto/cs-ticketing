using System.Net;
using CustomerServiceApp.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CustomerServiceApp.IntegrationTests.API.Controllers;

[Collection("Sequential Integration Tests")]
public class TicketsControllerIntegrationTests : ApiIntegrationTestBase
{
    public TicketsControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTicket_WithValidData_ReturnsCreated()
    {
        await AuthenticateAsPlayerAsync();
        var createTicketDto = new CreateTicketDto(
            "Integration Test Ticket",
            "This is a test ticket created during integration testing.",
            Guid.Empty); // This will be ignored and replaced with authenticated user ID

        var response = await PostAsync("/api/tickets", createTicketDto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await DeserializeAsync<TicketDto>(response);
        Assert.Equal(createTicketDto.Title, result.Title);
        Assert.Equal(createTicketDto.Description, result.Description);
        Assert.Equal("Open", result.Status);
    }

    [Fact]
    public async Task CreateTicket_WithoutAuthentication_ReturnsUnauthorized()
    {
        ClearAuthentication();
        var createTicketDto = new CreateTicketDto(
            "Integration Test Ticket",
            "This is a test ticket created during integration testing.",
            Guid.NewGuid()); // Any ID is fine since this should fail at auth level

        var response = await PostAsync("/api/tickets", createTicketDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_AsAgent_ReturnsForbidden()
    {
        await AuthenticateAsAgent1Async();
        var createTicketDto = new CreateTicketDto(
            "Test Ticket",
            "Test Description",
            Guid.Empty); // This will be ignored

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
        await AuthenticateAsAgent1Async();


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
        var ticketDto = await CreateSampleTicketAsync(); // Create a ticket first (authenticates as player)
        var playerId = ticketDto.Creator.Id; // Use the player ID from the created ticket
        // Note: Still authenticated as player from CreateSampleTicketAsync

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
        var playerId = Guid.NewGuid(); // Any ID is fine since this should fail at auth level

        var response = await GetAsync($"/api/tickets/player/{playerId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithValidData_ReturnsOk()
    {
        await ResetDatabaseAsync();

        await AuthenticateAsAgent1Async(); // Switch to agent to reply

        var createReplyDto = new CreateReplyDto(
            "This is a reply from the agent.",
            Agent1Id,
            Ticket2Id);

        var response = await PostAsync($"/api/tickets/{Ticket2Id}/replies", createReplyDto);


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithInvalidTicketId_ReturnsNotFound()
    {
        await ResetDatabaseAsync();

        await AuthenticateAsPlayerAsync();
        var invalidTicketId = Guid.NewGuid();

        var createReplyDto = new CreateReplyDto(
            "This reply should fail.",
            Guid.Empty, // This will be ignored and replaced with authenticated user ID
            invalidTicketId);


        var response = await PostAsync($"/api/tickets/{invalidTicketId}/replies", createReplyDto);


        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddReply_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var ticketId = Guid.NewGuid();

        var createReplyDto = new CreateReplyDto(
            "Unauthorized reply.",
            Guid.NewGuid(),
            ticketId);


        var response = await PostAsync($"/api/tickets/{ticketId}/replies", createReplyDto);


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_AsAgent_ReturnsOk()
    {

        var ticket = await CreateSampleTicketAsync();
        
        // First, add an agent reply to change status to "InResolution"
        await AuthenticateAsAgent1Async();
        var createReplyDto = new CreateReplyDto(
            "Agent reply to move to InResolution status.",
            Guid.Empty, // This will be ignored and replaced with authenticated user ID
            ticket.Id);
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

        await AuthenticateAsAgent1Async();
        var invalidTicketId = Guid.NewGuid();


        var response = await PutAsync($"/api/tickets/{invalidTicketId}/resolve", new { });


        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ResolveTicket_WithoutAuthentication_ReturnsUnauthorized()
    {
        ClearAuthentication();
        var ticketId = Guid.NewGuid(); // Any ID is fine since this should fail at auth level

        var response = await PutAsync($"/api/tickets/{ticketId}/resolve", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_WithInvalidData_ReturnsBadRequest()
    {
        await ResetDatabaseAsync();

        await AuthenticateAsPlayerAsync();
        var createTicketDto = new CreateTicketDto(
            "", // Invalid - empty title
            "Valid description",
            Guid.Empty); // This will be ignored


        var response = await PostAsync("/api/tickets", createTicketDto);


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
