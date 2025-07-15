using System.Net;
using System.Security.Claims;
using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Tickets;
using CustomerServiceApp.Application.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task GetTicket_WithExpiredToken_ReturnsUnauthorized()
    {
        // Reset database for clean test state
        await ResetDatabaseAsync();
        
        // Create an expired token manually
        var expiredToken = await CreateExpiredTokenAsync();
        
        // Set the expired token as authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);
        
        // Try to access a protected endpoint
        var ticketId = Guid.NewGuid(); // Any ticket ID
        var response = await GetAsync($"/api/tickets/{ticketId}");
        
        // Should return Unauthorized due to expired token
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Creates an expired JWT token for testing purposes
    /// </summary>
    private async Task<string> CreateExpiredTokenAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        
        // Create a player DTO for token generation
        var playerDto = new PlayerDto(
            Player1Id,
            Player1Email,
            Player1Name,
            Player1Number);
        
        // Generate a token that will be expired
        var token = jwtTokenService.GenerateToken(playerDto);
        
        // Wait for a short period to ensure token expires
        // Since we can't easily modify the token expiry time in production code,
        // we'll use a different approach: create a token with test configuration
        // that has very short expiry
        return await CreateShortLivedTokenAsync(playerDto);
    }

    /// <summary>
    /// Creates a token with very short expiry time for testing
    /// </summary>
    private async Task<string> CreateShortLivedTokenAsync(PlayerDto playerDto)
    {
        // Create token manually with 1 second expiry using JWT library directly
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.ASCII.GetBytes("ThisIsATestSecretKeyForIntegrationTestsOnly32Characters");
        
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, playerDto.Id.ToString()),
                new Claim(ClaimTypes.Email, playerDto.Email),
                new Claim(ClaimTypes.Name, playerDto.Name),
                new Claim(ClaimTypes.Role, playerDto.UserType)
            }),
            Expires = DateTime.UtcNow.AddSeconds(1), // 1 second expiry
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature),
            Issuer = "CustomerServiceApp",
            Audience = "CustomerServiceApp.Users"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        
        // Wait for 2 seconds to ensure token is expired
        await Task.Delay(2000);
        
        return tokenString;
    }
}
