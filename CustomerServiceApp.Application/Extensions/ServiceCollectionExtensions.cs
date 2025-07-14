using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Tickets;
using CustomerServiceApp.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerServiceApp.Application.Extensions;

/// <summary>
/// Service collection extensions for Application layer
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Application layer services
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITicketService, TicketService>();

        return services;
    }
}
