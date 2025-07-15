using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Infrastructure.Data;
using CustomerServiceApp.Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomerServiceApp.IntegrationTests.API;

/// <summary>
/// Base class for API integration tests providing common setup and helper methods
/// </summary>
public abstract class ApiIntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected ApiIntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Override logging to reduce noise during testing
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
            });
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add test configuration for JWT and password hashing
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JWT:SecretKey"] = "ThisIsATestSecretKeyForIntegrationTestsOnly32Characters",
                    ["JWT:Issuer"] = "CustomerServiceApp",
                    ["JWT:Audience"] = "CustomerServiceApp.Users",
                    ["JWT:ExpiryMinutes"] = "60",
                    ["PasswordHasher:Salt"] = "ThisIsATestSaltForIntegrationTestsOnly",
                    ["PasswordHasher:Iterations"] = "10000" // Lower for testing performance
                });
            });
        });

        Client = Factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Initialize database with seed data
        InitializeDatabaseAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerServiceDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Force seed data even if data exists (for testing isolation)
        await SeedTestDataAsync(context, passwordHasher);
    }

    private static async Task SeedTestDataAsync(CustomerServiceDbContext context, IPasswordHasher passwordHasher)
    {
        // Clear existing data to ensure test isolation
        context.Users.RemoveRange(context.Users);
        context.Tickets.RemoveRange(context.Tickets);
        context.Replies.RemoveRange(context.Replies);
        await context.SaveChangesAsync();

        // Create test players with explicit password verification
        var testPassword = "password123";
        var hashedPassword = passwordHasher.HashPassword(testPassword);
        
        // Verify the password can be verified
        var canVerify = passwordHasher.VerifyPassword(testPassword, hashedPassword);
        if (!canVerify)
        {
            throw new InvalidOperationException("Password hashing verification failed during test setup");
        }

        var player1 = new Domain.Users.Player
        {
            Email = "player1@example.com",
            Name = "John Doe",
            PasswordHash = hashedPassword,
            PlayerNumber = "P001"
        };

        var player2 = new Domain.Users.Player
        {
            Email = "player2@example.com",
            Name = "Jane Smith",
            PasswordHash = passwordHasher.HashPassword("password456"),
            PlayerNumber = "P002"
        };

        // Create test agent
        var agent1 = new Domain.Users.Agent
        {
            Email = "agent1@example.com",
            Name = "Agent Anderson",
            PasswordHash = passwordHasher.HashPassword("agentpass123")
        };

        context.Users.AddRange(player1, player2, agent1);
        await context.SaveChangesAsync();

        // Create sample tickets
        var ticket1 = new Domain.Tickets.Ticket
        {
            Title = "Login Issue",
            Description = "Cannot log into my account",
            Creator = player1
        };

        var ticket2 = new Domain.Tickets.Ticket
        {
            Title = "Payment Problem",
            Description = "Payment not processing correctly",
            Creator = player2
        };

        context.Tickets.AddRange(ticket1, ticket2);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Authenticates as a player and sets the Authorization header
    /// </summary>
    protected async Task<string> AuthenticateAsPlayerAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "password123"
        };

        var response = await PostAsync("/api/authentication/player/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await DeserializeAsync<AuthenticationResultDto>(response);
        var token = result.Token;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    /// <summary>
    /// Authenticates as an agent and sets the Authorization header
    /// </summary>
    protected async Task<string> AuthenticateAsAgentAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "agent1@example.com",
            Password = "agentpass123"
        };

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await DeserializeAsync<AuthenticationResultDto>(response);
        var token = result.Token;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    /// <summary>
    /// Clears authentication headers
    /// </summary>
    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Performs a GET request and returns the response
    /// </summary>
    protected async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        return await Client.GetAsync(requestUri);
    }

    /// <summary>
    /// Performs a POST request with JSON content
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PostAsync(requestUri, httpContent);
    }

    /// <summary>
    /// Performs a PUT request with JSON content
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync<T>(string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PutAsync(requestUri, httpContent);
    }

    /// <summary>
    /// Deserializes HTTP response content to the specified type
    /// </summary>
    protected async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
    }

    /// <summary>
    /// Gets error message from HTTP response
    /// </summary>
    protected async Task<string> GetErrorMessageAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Creates a sample ticket for testing purposes
    /// </summary>
    protected async Task<TicketDto> CreateSampleTicketAsync()
    {
        await AuthenticateAsPlayerAsync();

        var createTicketDto = new CreateTicketDto
        {
            CreatorId = new Guid("11111111-1111-1111-1111-111111111111"), // Player1 ID from seed data
            Title = "Integration Test Ticket",
            Description = "This is a test ticket created during integration testing."
        };

        var response = await PostAsync("/api/tickets", createTicketDto);
        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<TicketDto>(response);
    }

    /// <summary>
    /// Disposes the HTTP client
    /// </summary>
    public void Dispose()
    {
        Client.Dispose();
    }
}
