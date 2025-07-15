using System.Net;
using CustomerServiceApp.Application.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerServiceApp.IntegrationTests.API.Controllers;

public class AuthenticationControllerIntegrationTests : ApiIntegrationTestBase
{
    public AuthenticationControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task PlayerLogin_WithValidCredentials_ReturnsOkWithToken()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "Player123!"
        };

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
        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "WrongPassword"
        };

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithAgentCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "agent1@example.com",
            Password = "agentpass123"
        };

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithValidCredentials_ReturnsOkWithToken()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "agent1@example.com",
            Password = "agentpass123"
        };

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
        var loginRequest = new LoginRequestDto
        {
            Email = "agent1@example.com",
            Password = "WrongPassword"
        };

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithPlayerCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "password123"
        };

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "", // Invalid - empty email
            Password = "Player123!"
        };

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "agent@customerservice.com",
            Password = "" // Invalid - empty password
        };

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithNonExistentUser_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithNonExistentUser_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
