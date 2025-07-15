using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
    private readonly string _databaseName;
    // Test entity IDs
    protected static readonly Guid Player1Id = new("11111111-1111-1111-1111-111111111111");
    protected static readonly Guid Player2Id = new("22222222-2222-2222-2222-222222222222");
    protected static readonly Guid Agent1Id = new("33333333-3333-3333-3333-333333333333");
    protected static readonly Guid Agent2Id = new("44444444-4444-4444-4444-444444444444");
    protected static readonly Guid Ticket1Id = new("55555555-5555-5555-5555-555555555555");
    protected static readonly Guid Ticket2Id = new("66666666-6666-6666-6666-666666666666");

    // Test data constants
    protected const string TestPassword1 = "password123";
    protected const string TestPassword2 = "password456";
    protected const string TestAgentPassword1 = "agentpass123";
    protected const string TestAgentPassword2 = "agentpass456";

    // Test user data
    protected static readonly string Player1Email = "player1@example.com";
    protected static readonly string Player1Name = "John Doe";
    protected static readonly string Player1Number = "P001";

    protected static readonly string Player2Email = "player2@example.com";
    protected static readonly string Player2Name = "Jane Smith";
    protected static readonly string Player2Number = "P002";

    protected static readonly string Agent1Email = "agent1@example.com";
    protected static readonly string Agent1Name = "Agent Anderson";

    protected static readonly string Agent2Email = "agent@customerservice.com";
    protected static readonly string Agent2Name = "Customer Service Agent";

    // Test ticket data
    protected static readonly string Ticket1Title = "Login Issue";
    protected static readonly string Ticket1Description = "Cannot log into my account";

    protected static readonly string Ticket2Title = "Payment Problem";
    protected static readonly string Ticket2Description = "Payment not processing correctly";

    protected ApiIntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        // Use a consistent database name for all tests to enable sequential execution
        _databaseName = "IntegrationTestDb";
        
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Override logging to reduce noise during testing
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
                
                // Remove existing DbContext registration and add unique one
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(CustomerServiceDbContext));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                // Add DbContext with consistent In-Memory database for sequential tests
                services.AddDbContext<CustomerServiceDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Add test configuration for JWT and password hashing
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtToken:SecretKey"] = "ThisIsATestSecretKeyForIntegrationTestsOnly32Characters",
                    ["JwtToken:Issuer"] = "CustomerServiceApp",
                    ["JwtToken:Audience"] = "CustomerServiceApp.Users",
                    ["JwtToken:ExpiryMinutes"] = "60",
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

        // Initialize fresh database for this test
        InitializeCleanDatabaseAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeCleanDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerServiceDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        
        // Always clean and recreate database for each test to ensure isolation
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Seed fresh test data
        await SeedTestDataAsync(context, passwordHasher);
    }

    /// <summary>
    /// Resets the database for the next test - call this at the beginning of each test method
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerServiceDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        
        // Clear authentication before resetting database
        ClearAuthentication();
        
        // Clean and recreate database
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Seed fresh test data
        await SeedTestDataAsync(context, passwordHasher);
    }

    private static async Task SeedTestDataAsync(CustomerServiceDbContext context, IPasswordHasher passwordHasher)
    {
        try
        {
            // Create test players with explicit password verification
            var hashedPassword1 = passwordHasher.HashPassword(TestPassword1);
            
            // Verify the password can be verified
            var canVerify = passwordHasher.VerifyPassword(TestPassword1, hashedPassword1);
            if (!canVerify)
            {
                throw new InvalidOperationException("Password hashing verification failed during test setup");
            }

            var player1 = new Domain.Users.Player
            {
                Id = Player1Id,
                Email = Player1Email,
                Name = Player1Name,
                PasswordHash = hashedPassword1,
                PlayerNumber = Player1Number
            };

            var player2 = new Domain.Users.Player
            {
                Id = Player2Id,
                Email = Player2Email,
                Name = Player2Name,
                PasswordHash = passwordHasher.HashPassword(TestPassword2),
                PlayerNumber = Player2Number
            };

            // Create test agents
            var agent1 = new Domain.Users.Agent
            {
                Id = Agent1Id,
                Email = Agent1Email,
                Name = Agent1Name,
                PasswordHash = passwordHasher.HashPassword(TestAgentPassword1)
            };

            var agent2 = new Domain.Users.Agent
            {
                Id = Agent2Id,
                Email = Agent2Email,
                Name = Agent2Name,
                PasswordHash = passwordHasher.HashPassword(TestAgentPassword2)
            };

            context.Users.AddRange(player1, player2, agent1, agent2);
            await context.SaveChangesAsync();

            // Create sample tickets
            var ticket1 = new Domain.Tickets.Ticket
            {
                Id = Ticket1Id,
                Title = Ticket1Title,
                Description = Ticket1Description,
                Creator = player1
            };

            var ticket2 = new Domain.Tickets.Ticket
            {
                Id = Ticket2Id,
                Title = Ticket2Title,
                Description = Ticket2Description,
                Creator = player2
            };

            context.Tickets.AddRange(ticket1, ticket2);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log and re-throw any seeding errors
            throw new InvalidOperationException($"Failed to seed test data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Authenticates as a player and sets the Authorization header
    /// </summary>
    protected async Task<AuthenticationResultDto> AuthenticateAsPlayerAsync()
    {
        var loginRequest = new LoginRequestDto(Player1Email, TestPassword1);

        var response = await PostAsync("/api/authentication/player/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await DeserializeAsync<AuthenticationResultDto>(response);
        var token = result.Token;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return result;
    }

    /// <summary>
    /// Authenticates as an agent and sets the Authorization header
    /// </summary>
    protected async Task<string> AuthenticateAsAgent1Async()
    {
        var loginRequest = new LoginRequestDto(Agent1Email, TestAgentPassword1);

        var response = await PostAsync("/api/authentication/agent/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await DeserializeAsync<AuthenticationResultDto>(response);
        var token = result.Token;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    /// <summary>
    /// Clears the authentication header for testing unauthenticated scenarios
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

        var createTicketDto = new CreateTicketDto(
            "Integration Test Ticket",
            "This is a test ticket created during integration testing.",
            Guid.NewGuid() // This will be ignored and replaced with authenticated user ID
        );

        var response = await PostAsync("/api/tickets", createTicketDto);
        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<TicketDto>(response);
    }

    /// <summary>
    /// Creates a sample ticket for testing purposes without changing authentication
    /// </summary>
    protected async Task<TicketDto> CreateSampleTicketWithoutAuthAsync()
    {
        // Save current authentication state
        var currentAuth = Client.DefaultRequestHeaders.Authorization;
        
        try
        {
            // Temporarily authenticate as player to create ticket
            await AuthenticateAsPlayerAsync();

            var createTicketDto = new CreateTicketDto(
                "Integration Test Ticket",
                "This is a test ticket created during integration testing.",
                Guid.Empty // This will be ignored and replaced with authenticated user ID
            );

            var response = await PostAsync("/api/tickets", createTicketDto);
            response.EnsureSuccessStatusCode();

            return await DeserializeAsync<TicketDto>(response);
        }
        finally
        {
            // Restore previous authentication state
            Client.DefaultRequestHeaders.Authorization = currentAuth;
        }
    }

    /// <summary>
    /// Disposes the HTTP client
    /// </summary>
    public void Dispose()
    {
        Client.Dispose();
    }
}
