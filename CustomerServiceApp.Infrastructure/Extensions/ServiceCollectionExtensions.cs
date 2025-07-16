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

        const string devPassword = "password123"; // Development password
        // Create sample players
        var player1 = new Domain.Users.Player()
        {
            Email = "player1@example.com",
            Name = "John Doe",
            PasswordHash = passwordHasher.HashPassword(devPassword), // Development password
            PlayerNumber = "P001",
            Avatar = "/img/player1.jpg"
        };

        var player2 = new Domain.Users.Player
        {
            Email = "player2@example.com",
            Name = "Jane Smith",
            PasswordHash = passwordHasher.HashPassword(devPassword), // Development password
            PlayerNumber = "P002",
            Avatar = "/img/player2.jpg"
        };

        var player3 = new Domain.Users.Player
        {
            Email = "player3@example.com",
            Name = "Bob Johnson",
            PasswordHash = passwordHasher.HashPassword(devPassword), // Development password
            PlayerNumber = "P003",
        };

        // Create sample agent
        var agent1 = new Domain.Users.Agent
        {
            Email = "agent1@example.com",
            Name = "CS Agent",
            PasswordHash = passwordHasher.HashPassword(devPassword), // Development password
            Avatar = "/img/agent1.jpg"
        };
        var agent2 = new Domain.Users.Agent
        {
            Email = "agent2@example.com",
            Name = "CS Agent 2",
            PasswordHash = passwordHasher.HashPassword(devPassword), // Development password
            Avatar = "/img/agent2.jpg"
        };
        var agent3 = new Domain.Users.Agent
        {
            Email = "agent3@example.com",
            Name = "CS Agent 3",
            PasswordHash = passwordHasher.HashPassword(devPassword), // Development password
            Avatar = "/img/agent3.jpg"
        };

        context.Users.AddRange(player1, player2, player3, agent1, agent2, agent3);
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

        // Create resolved ticket 1: Player1 ticket resolved by Agent2
        var resolvedTicket1 = new Domain.Tickets.Ticket
        {
            Title = "Account Verification",
            Description = "Need help verifying my account details",
            Creator = player1
        };

        // Create resolved ticket 2: Player2 ticket with Agent1 reply, then resolved by Agent3
        var resolvedTicket2 = new Domain.Tickets.Ticket
        {
            Title = "Game Balance Issue",
            Description = "My game balance is showing incorrect amount",
            Creator = player2
        };

        context.Tickets.AddRange(ticket1, ticket2, resolvedTicket1, resolvedTicket2);
        await context.SaveChangesAsync();

        // Add sample replies
        var reply1 = new Domain.Tickets.Reply
        {
            Content = "We're looking into this issue. Can you provide more details?",
            Author = agent1,
            TicketId = ticket1.Id
        };
        await Task.Delay(10);
        var reply2 = new Domain.Tickets.Reply
        {
            Content = "I tried resetting my password but it still doesn't work.",
            Author = player1,
            TicketId = ticket1.Id
        };
        await Task.Delay(10);
        // Replies for resolved ticket 1 (Player1 ticket resolved by Agent2)
        var resolvedReply1 = new Domain.Tickets.Reply
        {
            Content = "I can help you with account verification. Please provide your email and player number.",
            Author = agent2,
            TicketId = resolvedTicket1.Id
        };
        await Task.Delay(10);
        var resolvedReply2 = new Domain.Tickets.Reply
        {
            Content = "My email is player1@example.com and player number is P001.",
            Author = player1,
            TicketId = resolvedTicket1.Id
        };
        await Task.Delay(10);
        var resolvedReply3 = new Domain.Tickets.Reply
        {
            Content = "Thank you. I've verified your account successfully. Everything looks good now.",
            Author = agent2,
            TicketId = resolvedTicket1.Id
        };
        await Task.Delay(10);
        // Replies for resolved ticket 2 (Player2 ticket with Agent1 reply, then resolved by Agent3)
        var resolvedReply4 = new Domain.Tickets.Reply
        {
            Content = "I can see the balance discrepancy. Let me investigate this for you.",
            Author = agent1,
            TicketId = resolvedTicket2.Id
        };
        await Task.Delay(10);
        var resolvedReply5 = new Domain.Tickets.Reply
        {
            Content = "Thank you! When can I expect this to be fixed?",
            Author = player2,
            TicketId = resolvedTicket2.Id
        };
        await Task.Delay(10);
        var resolvedReply6 = new Domain.Tickets.Reply
        {
            Content = "I've reviewed the case and corrected your balance. The issue has been resolved.",
            Author = agent3,
            TicketId = resolvedTicket2.Id
        };

        context.Replies.AddRange(reply1, reply2, resolvedReply1, resolvedReply2, resolvedReply3, 
                                 resolvedReply4, resolvedReply5, resolvedReply6);

        // Add replies to ticket1 (existing open ticket)
        ticket1.AddReply(reply1);
        ticket1.AddReply(reply2);

        // Add replies and resolve ticket1 (Player1 ticket resolved by Agent2)
        resolvedTicket1.AddReply(resolvedReply1);
        resolvedTicket1.AddReply(resolvedReply2);
        resolvedTicket1.AddReply(resolvedReply3);
        resolvedTicket1.Resolve(agent2);

        // Add replies and resolve ticket2 (Player2 ticket with Agent1 reply, then resolved by Agent3)
        resolvedTicket2.AddReply(resolvedReply4);
        resolvedTicket2.AddReply(resolvedReply5);
        resolvedTicket2.AddReply(resolvedReply6);
        resolvedTicket2.Resolve(agent3);

        context.Tickets.UpdateRange(ticket1, resolvedTicket1, resolvedTicket2);
        await context.SaveChangesAsync();
    }
}
