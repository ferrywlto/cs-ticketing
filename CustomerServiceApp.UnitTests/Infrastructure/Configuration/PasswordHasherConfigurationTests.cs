using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Infrastructure.Extensions;
using CustomerServiceApp.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CustomerServiceApp.UnitTests.Infrastructure.Configuration;

public class PasswordHasherConfigurationTests
{
    [Fact]
    public void AddInfrastructure_Should_Throw_When_Salt_Is_Missing()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IPasswordHasher>());

        Assert.Contains("PasswordHasher:Salt is required", exception.Message);
        Assert.Contains("dotnet user-secrets set", exception.Message);
    }

    [Fact]
    public void AddInfrastructure_Should_Throw_When_Salt_Is_Too_Short()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "PasswordHasher:Salt", "short" }, // Less than 32 characters
                { "PasswordHasher:Iterations", "100000" }
            })
            .Build();

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IPasswordHasher>());

        Assert.Contains("must be at least 32 characters long", exception.Message);
    }

    [Fact]
    public void AddInfrastructure_Should_Throw_When_Iterations_Are_Too_Low()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "PasswordHasher:Salt", "ThisIsAValidSaltThatIs32CharactersLong12345" },
                { "PasswordHasher:Iterations", "5000" } // Too low
            })
            .Build();

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IPasswordHasher>());

        Assert.Contains("must be between 10,000 and 1,000,000", exception.Message);
    }

    [Fact]
    public void AddInfrastructure_Should_Throw_When_Iterations_Are_Too_High()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "PasswordHasher:Salt", "ThisIsAValidSaltThatIs32CharactersLong12345" },
                { "PasswordHasher:Iterations", "2000000" } // Too high
            })
            .Build();

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IPasswordHasher>());

        Assert.Contains("must be between 10,000 and 1,000,000", exception.Message);
    }

    [Fact]
    public void AddInfrastructure_Should_Succeed_With_Valid_Configuration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "PasswordHasher:Salt", "ThisIsAValidSaltThatIs32CharactersLong12345" },
                { "PasswordHasher:Iterations", "100000" }
            })
            .Build();

        // Should not throw
        services.AddInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<PasswordHasherOptions>>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>(); // Should not throw

        Assert.Equal("ThisIsAValidSaltThatIs32CharactersLong12345", options.Value.Salt);
        Assert.Equal(100000, options.Value.Iterations);
        Assert.NotNull(passwordHasher);
    }

    [Fact]
    public void AddInfrastructure_Should_Use_Default_Iterations_When_Not_Specified()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "PasswordHasher:Salt", "ThisIsAValidSaltThatIs32CharactersLong12345" }
                // No iterations specified - should use default
            })
            .Build();

        services.AddInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<PasswordHasherOptions>>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>(); // Should not throw

        Assert.Equal("ThisIsAValidSaltThatIs32CharactersLong12345", options.Value.Salt);
        Assert.Equal(100000, options.Value.Iterations); // Default value
        Assert.NotNull(passwordHasher);
    }
}
