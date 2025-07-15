using System.Net;
using System.Text;
using System.Text.Json;
using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Infrastructure.Data;
using CustomerServiceApp.IntegrationTests.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerServiceApp.IntegrationTests.API.Controllers;

public class AuthenticationDebugTests : ApiIntegrationTestBase
{
    public AuthenticationDebugTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public void Debug_PasswordHashing_Works()
    {
        // Get the password hasher from DI
        using var scope = Factory.Services.CreateScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var context = scope.ServiceProvider.GetRequiredService<CustomerServiceDbContext>();

        // Test password hashing directly
        var testPassword = "password123";
        var hashedPassword = passwordHasher.HashPassword(testPassword);
        
        // Verify it can be verified
        var canVerify = passwordHasher.VerifyPassword(testPassword, hashedPassword);
        Assert.True(canVerify, "Password verification should work");

        // Check if the user exists in the database
        var user = context.Users.FirstOrDefault(u => u.Email == "player1@example.com");
        Assert.NotNull(user);
        
        // Verify the stored password hash works
        var storedPasswordWorks = passwordHasher.VerifyPassword(testPassword, user.PasswordHash);
        Assert.True(storedPasswordWorks, "Stored password hash should verify correctly");
    }

    [Fact]
    public async Task Debug_AuthenticationService_Directly()
    {
        // Get the authentication service from DI
        using var scope = Factory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        var loginRequest = new LoginRequestDto
        {
            Email = "player1@example.com",
            Password = "password123"
        };

        var result = await authService.LoginAsync(loginRequest);
        
        Assert.True(result.IsSuccess, $"Authentication should succeed. Error: {result.Error}");
        Assert.NotNull(result.Data);
        Assert.Equal("player1@example.com", result.Data.User.Email);
    }
}
