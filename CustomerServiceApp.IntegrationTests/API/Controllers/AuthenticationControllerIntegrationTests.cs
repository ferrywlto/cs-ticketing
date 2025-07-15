using System.Net;
using CustomerServiceApp.Application.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CustomerServiceApp.IntegrationTests.API.Controllers;

[Collection("Sequential Integration Tests")]
public class AuthenticationControllerIntegrationTests : ApiIntegrationTestBase
{
    public AuthenticationControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task PlayerLogin_WithValidCredentials_ReturnsOkWithToken()
    {
        // Reset database for clean test state
        await ResetDatabaseAsync();
        
        var loginRequest = new LoginRequestDto("player1@example.com", "password123");

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<AuthenticationResultDto>(response);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("player1@example.com", result.User.Email);
        Assert.Equal("Player", result.User.UserType);
    }

    [Fact]
    public async Task PlayerLogin_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto("player1@example.com", "WrongPassword");

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithAgentCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto("agent1@example.com", "agentpass123");

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithValidCredentials_ReturnsOkWithToken()
    {
        var loginRequest = new LoginRequestDto("agent1@example.com", "agentpass123");

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await DeserializeAsync<AuthenticationResultDto>(response);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(result.User);
        Assert.Equal("agent1@example.com", result.User.Email);
        Assert.Equal("Agent", result.User.UserType);
    }

    [Fact]
    public async Task AgentLogin_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto("agent1@example.com", "WrongPassword");

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithPlayerCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto("player1@example.com", "password123");

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto("", "Player123!"); // Invalid - empty email

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto("agent@customerservice.com", ""); // Invalid - empty password

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithNonExistentUser_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto("nonexistent@example.com", "SomePassword123!");

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithNonExistentUser_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto("nonexistent@example.com", "SomePassword123!");

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
