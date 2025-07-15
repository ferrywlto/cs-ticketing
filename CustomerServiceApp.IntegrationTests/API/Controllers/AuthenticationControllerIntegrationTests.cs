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
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "Player123!"
        };

        // Act
        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        // Assert
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
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithAgentCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "agent1@example.com",
            Password = "agentpass123"
        };

        // Act
        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "agent1@example.com",
            Password = "agentpass123"
        };

        // Act
        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        // Assert
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
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "agent1@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithPlayerCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "password123"
        };

        // Act
        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "", // Invalid - empty email
            Password = "Player123!"
        };

        // Act
        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "agent@customerservice.com",
            Password = "" // Invalid - empty password
        };

        // Act
        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlayerLogin_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        // Act
        var response = await PostAsync("/api/authentication/player/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AgentLogin_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        // Act
        var response = await PostAsync("/api/authentication/agent/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
