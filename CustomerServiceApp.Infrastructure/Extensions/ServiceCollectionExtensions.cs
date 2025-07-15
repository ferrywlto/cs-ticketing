using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Users;
using CustomerServiceApp.Application.Tickets;
using CustomerServiceApp.Infrastructure.Data;
using CustomerServiceApp.Infrastructure.Options;
using CustomerServiceApp.Infrastructure.Repositories;
using CustomerServiceApp.Infrastructure.Services;

namespace CustomerServiceApp.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure password hasher options from configuration with validation
        services.Configure<PasswordHasherOptions>(passwordHasherOptions =>
        {
            var section = configuration.GetSection(PasswordHasherOptions.SectionName);
            section.Bind(passwordHasherOptions);

            // Validate that required configuration is present
            if (string.IsNullOrWhiteSpace(passwordHasherOptions.Salt))
            {
                throw new InvalidOperationException(
                    "PasswordHasher:Salt is required. " +
                    "For development, set it using: dotnet user-secrets set \"PasswordHasher:Salt\" \"your-secure-salt-here\" " +
                    "For production, set it via environment variables or appsettings.json");
            }

            if (passwordHasherOptions.Salt.Length < 32)
            {
                throw new InvalidOperationException(
                    "PasswordHasher:Salt must be at least 32 characters long for security.");
            }

            if (passwordHasherOptions.Iterations < 10000 || passwordHasherOptions.Iterations > 1000000)
            {
                throw new InvalidOperationException(
                    "PasswordHasher:Iterations must be between 10,000 and 1,000,000.");
            }
        });

        // Configure JWT token options from configuration with validation
        services.Configure<JwtTokenOptions>(jwtTokenOptions =>
        {
            var section = configuration.GetSection("JwtToken");
            section.Bind(jwtTokenOptions);

            // Validate that required configuration is present
            if (string.IsNullOrWhiteSpace(jwtTokenOptions.SecretKey))
            {
                throw new InvalidOperationException(
                    "JwtToken:SecretKey is required. " +
                    "For development, set it using: dotnet user-secrets set \"JwtToken:SecretKey\" \"your-secure-secret-key-here\" " +
                    "For production, set it via environment variables or appsettings.json");
            }

            if (jwtTokenOptions.SecretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "JwtToken:SecretKey must be at least 32 characters long for security.");
            }

            if (string.IsNullOrWhiteSpace(jwtTokenOptions.Issuer))
            {
                throw new InvalidOperationException("JWT:Issuer is required.");
            }

            if (string.IsNullOrWhiteSpace(jwtTokenOptions.Audience))
            {
                throw new InvalidOperationException("JWT:Audience is required.");
            }

            if (jwtTokenOptions.ExpiryMinutes <= 0 || jwtTokenOptions.ExpiryMinutes > 1440) // max 24 hours
            {
                throw new InvalidOperationException(
                    "JWT:ExpiryMinutes must be between 1 and 1440 minutes (24 hours).");
            }
        });

        // Add DbContext with In-Memory database
        services.AddDbContext<CustomerServiceDbContext>(options =>
            options.UseInMemoryDatabase("CustomerServiceDb"));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register application services
        services.AddScoped<UserService>();
        services.AddScoped<TicketService>();

        // Register infrastructure services
        services.AddScoped<IMapper, Mapper>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    /// <summary>
    /// Ensures the database is created and seeded with initial data
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, bool seedData = false)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerServiceDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed initial data if needed
        if (seedData)
        {
            await SeedDataAsync(context, passwordHasher);
        }
    }

    private static async Task SeedDataAsync(CustomerServiceDbContext context, IPasswordHasher passwordHasher)
    {
        // Check if data already exists
        if (context.Users.Any())
            return;

        // Create sample players
        var player1 = new CustomerServiceApp.Domain.Users.Player
        {
            Email = "player1@example.com",
            Name = "John Doe",
            PasswordHash = passwordHasher.HashPassword("password123"), // Development password
            PlayerNumber = "P001"
        };

        var player2 = new CustomerServiceApp.Domain.Users.Player
        {
            Email = "player2@example.com",
            Name = "Jane Smith",
            PasswordHash = passwordHasher.HashPassword("password456"), // Development password
            PlayerNumber = "P002"
        };

        var player3 = new CustomerServiceApp.Domain.Users.Player
        {
            Email = "player3@example.com",
            Name = "Bob Johnson",
            PasswordHash = passwordHasher.HashPassword("password789"), // Development password
            PlayerNumber = "P003"
        };

        // Create sample agent
        var agent1 = new CustomerServiceApp.Domain.Users.Agent
        {
            Email = "agent1@example.com",
            Name = "Agent Anderson",
            PasswordHash = passwordHasher.HashPassword("agentpass123") // Development password
        };

        context.Users.AddRange(player1, player2, player3, agent1);
        await context.SaveChangesAsync();

        // Create sample tickets
        var ticket1 = new CustomerServiceApp.Domain.Tickets.Ticket
        {
            Title = "Login Issue",
            Description = "Cannot log into my account",
            Creator = player1
        };

        var ticket2 = new CustomerServiceApp.Domain.Tickets.Ticket
        {
            Title = "Payment Problem",
            Description = "Payment not processing correctly",
            Creator = player2
        };

        context.Tickets.AddRange(ticket1, ticket2);
        await context.SaveChangesAsync();

        // Add sample replies
        var reply1 = new CustomerServiceApp.Domain.Tickets.Reply
        {
            Content = "We're looking into this issue. Can you provide more details?",
            Author = agent1
        };

        ticket1.AddReply(reply1);

        var reply2 = new CustomerServiceApp.Domain.Tickets.Reply
        {
            Content = "I tried resetting my password but it still doesn't work.",
            Author = player1
        };

        ticket1.AddReply(reply2);

        context.Tickets.UpdateRange(ticket1);
        await context.SaveChangesAsync();
    }
}
