using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Users;
using CustomerServiceApp.Application.Tickets;
using CustomerServiceApp.Infrastructure.Data;
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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
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

        return services;
    }

    /// <summary>
    /// Ensures the database is created and seeded with initial data
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, bool seedData = false)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomerServiceDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed initial data if needed
        if (seedData)
        {
            await SeedDataAsync(context);
        }
    }

    private static async Task SeedDataAsync(CustomerServiceDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any())
            return;

        // Create sample players
        var player1 = new CustomerServiceApp.Domain.Users.Player
        {
            Email = "player1@example.com",
            Name = "John Doe",
            PasswordHash = "hashed_password_123", // In production, properly hash this
            PlayerNumber = "P001"
        };

        var player2 = new CustomerServiceApp.Domain.Users.Player
        {
            Email = "player2@example.com",
            Name = "Jane Smith",
            PasswordHash = "hashed_password_456",
            PlayerNumber = "P002"
        };

        // Create sample agent
        var agent1 = new CustomerServiceApp.Domain.Users.Agent
        {
            Email = "agent1@example.com",
            Name = "Agent Anderson",
            PasswordHash = "hashed_password_789"
        };

        context.Users.AddRange(player1, player2, agent1);
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
