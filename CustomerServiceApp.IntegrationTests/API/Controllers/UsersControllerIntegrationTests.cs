using System.Net;
using CustomerServiceApp.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerServiceApp.IntegrationTests.API.Controllers;

public class UsersControllerIntegrationTests : ApiIntegrationTestBase
{
    public UsersControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePlayer_AsAgent_ReturnsCreated()
    {

        await AuthenticateAsAgentAsync();
        var createPlayerDto = new CreatePlayerDto(
            "newplayer@example.com",
            "New Player",
            null,
            "Password123!",
            "P999");


        var response = await PostAsync("/api/users/players", createPlayerDto);


        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await DeserializeAsync<PlayerDto>(response);
        Assert.Equal(createPlayerDto.Email, result.Email);
        Assert.Equal(createPlayerDto.Name, result.Name);
        Assert.Equal(createPlayerDto.PlayerNumber, result.PlayerNumber);
        Assert.Equal("Player", result.UserType);
    }

    [Fact]
    public async Task CreatePlayer_AsPlayer_ReturnsForbidden()
    {

        await AuthenticateAsPlayerAsync();
        var createPlayerDto = new CreatePlayerDto(
            "newplayer@example.com",
            "New Player",
            null,
            "Password123!",
            "P999");


        var response = await PostAsync("/api/users/players", createPlayerDto);


        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlayer_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var createPlayerDto = new CreatePlayerDto(
            "newplayer@example.com",
            "New Player",
            null,
            "Password123!",
            "P999");


        var response = await PostAsync("/api/users/players", createPlayerDto);


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlayer_WithDuplicateEmail_ReturnsBadRequest()
    {

        await AuthenticateAsAgentAsync();
        var createPlayerDto = new CreatePlayerDto(
            "player1@example.com", // This email already exists in seed data
            "Duplicate Player",
            null,
            "Password123!",
            "P998");


        var response = await PostAsync("/api/users/players", createPlayerDto);


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlayer_WithInvalidData_ReturnsBadRequest()
    {

        await AuthenticateAsAgentAsync();
        var createPlayerDto = new CreatePlayerDto(
            "invalid-email", // Invalid email format
            "Test Player",
            null,
            "Password123!",
            "P997");


        var response = await PostAsync("/api/users/players", createPlayerDto);


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_AsAgent_ReturnsCreated()
    {

        await AuthenticateAsAgentAsync();
        var createAgentDto = new CreateAgentDto(
            "newagent@customerservice.com",
            "New Agent",
            null,
            "Password123!");


        var response = await PostAsync("/api/users/agents", createAgentDto);


        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await DeserializeAsync<AgentDto>(response);
        Assert.Equal(createAgentDto.Email, result.Email);
        Assert.Equal(createAgentDto.Name, result.Name);
        Assert.Equal("Agent", result.UserType);
    }

    [Fact]
    public async Task CreateAgent_AsPlayer_ReturnsForbidden()
    {

        await AuthenticateAsPlayerAsync();
        var createAgentDto = new CreateAgentDto(
            "newagent@customerservice.com",
            "New Agent",
            null,
            "Password123!");


        var response = await PostAsync("/api/users/agents", createAgentDto);


        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var createAgentDto = new CreateAgentDto(
            "newagent@customerservice.com",
            "New Agent",
            null,
            "Password123!");


        var response = await PostAsync("/api/users/agents", createAgentDto);


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_WithDuplicateEmail_ReturnsBadRequest()
    {

        await AuthenticateAsAgentAsync();
        var createAgentDto = new CreateAgentDto(
            "agent@customerservice.com", // This email already exists in seed data
            "Duplicate Agent",
            null,
            "Password123!");


        var response = await PostAsync("/api/users/agents", createAgentDto);


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsOk()
    {

        await AuthenticateAsAgentAsync();
        var playerId = new Guid("11111111-1111-1111-1111-111111111111"); // Player1 ID from seed data


        var response = await GetAsync($"/api/users/{playerId}");


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<PlayerDto>(response);
        Assert.Equal(playerId, result.Id);
        Assert.Equal("Player", result.UserType);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {

        await AuthenticateAsAgentAsync();
        var invalidId = Guid.NewGuid();


        var response = await GetAsync($"/api/users/{invalidId}");


        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_WithoutAuthentication_ReturnsUnauthorized()
    {

        ClearAuthentication();
        var userId = Guid.NewGuid(); // Any ID is fine since this should fail at auth level


        var response = await GetAsync($"/api/users/{userId}");


        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlayer_WithMissingRequiredFields_ReturnsBadRequest()
    {

        await AuthenticateAsAgentAsync();
        var createPlayerDto = new CreatePlayerDto(
            "test@example.com",
            "", // Missing required name
            null,
            "Password123!",
            "P996");


        var response = await PostAsync("/api/users/players", createPlayerDto);


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAgent_WithMissingRequiredFields_ReturnsBadRequest()
    {

        await AuthenticateAsAgentAsync();
        var createAgentDto = new CreateAgentDto(
            "test@example.com",
            "Test Agent",
            null,
            ""); // Missing required password


        var response = await PostAsync("/api/users/agents", createAgentDto);


        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
