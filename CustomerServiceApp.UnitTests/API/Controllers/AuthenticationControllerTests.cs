using CustomerServiceApp.API.Controllers;
using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerServiceApp.UnitTests.API.Controllers;

public class AuthenticationControllerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;
    private readonly Mock<ILogger<AuthenticationController>> _mockLogger;
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<AuthenticationController>>();
        _controller = new AuthenticationController(_mockAuthenticationService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task PlayerLogin_WithValidPlayerCredentials_ReturnsOkWithToken()
    {
        var loginRequest = new LoginRequestDto(
            "player@example.com",
            "password123");

        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            loginRequest.Email,
            "Test Player",
            "P001");

        var authResult = new AuthenticationResultDto(
            playerDto,
            "jwt.token.here",
            DateTime.UtcNow.AddHours(1));

        var result = Result<AuthenticationResultDto>.Success(authResult);
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        var response = await _controller.PlayerLogin(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(response);
        var returnedAuth = Assert.IsType<AuthenticationResultDto>(okResult.Value);
        Assert.Equal(authResult.Token, returnedAuth.Token);
        Assert.Equal(playerDto.Id, returnedAuth.User.Id);
        Assert.Equal("Player", returnedAuth.User.UserType);
    }

    [Fact]
    public async Task PlayerLogin_WithValidAgentCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto(
            "agent@example.com",
            "password123");

        var agentDto = new AgentDto(
            Guid.NewGuid(),
            loginRequest.Email,
            "Test Agent");

        var authResult = new AuthenticationResultDto(
            agentDto,
            "jwt.token.here",
            DateTime.UtcNow.AddHours(1));

        var result = Result<AuthenticationResultDto>.Success(authResult);
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        var response = await _controller.PlayerLogin(loginRequest);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
        var errorMessage = unauthorizedResult.Value;
        Assert.NotNull(errorMessage);
        
        // Verify warning was logged for cross-role attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Non-player user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PlayerLogin_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto(
            "invalid@example.com",
            "wrongpassword");

        var result = Result<AuthenticationResultDto>.Failure("Invalid email or password");
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        var response = await _controller.PlayerLogin(loginRequest);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
        var errorMessage = unauthorizedResult.Value;
        Assert.NotNull(errorMessage);

        // Verify warning was logged for failed login
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Player login failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PlayerLogin_WithException_ReturnsInternalServerError()
    {
        var loginRequest = new LoginRequestDto(
            "player@example.com",
            "password123");

        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ThrowsAsync(new Exception("Database connection failed"));

        var response = await _controller.PlayerLogin(loginRequest);

        var statusCodeResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, statusCodeResult.StatusCode);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during player login")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PlayerLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto(
            "", // Invalid email
            "password123");

        _controller.ModelState.AddModelError("Email", "Email is required.");

        var response = await _controller.PlayerLogin(loginRequest);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task AgentLogin_WithValidAgentCredentials_ReturnsOkWithToken()
    {
        var loginRequest = new LoginRequestDto(
            "agent@example.com",
            "password123");

        var agentDto = new AgentDto(
            Guid.NewGuid(),
            loginRequest.Email,
            "Test Agent");

        var authResult = new AuthenticationResultDto(
            agentDto,
            "jwt.token.here",
            DateTime.UtcNow.AddHours(1));

        var result = Result<AuthenticationResultDto>.Success(authResult);
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        var response = await _controller.AgentLogin(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(response);
        var returnedAuth = Assert.IsType<AuthenticationResultDto>(okResult.Value);
        Assert.Equal(authResult.Token, returnedAuth.Token);
        Assert.Equal(agentDto.Id, returnedAuth.User.Id);
        Assert.Equal("Agent", returnedAuth.User.UserType);
    }

    [Fact]
    public async Task AgentLogin_WithValidPlayerCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto(
            "player@example.com",
            "password123");

        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            loginRequest.Email,
            "Test Player",
            "P001");

        var authResult = new AuthenticationResultDto(
            playerDto,
            "jwt.token.here",
            DateTime.UtcNow.AddHours(1));

        var result = Result<AuthenticationResultDto>.Success(authResult);
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        var response = await _controller.AgentLogin(loginRequest);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
        var errorMessage = unauthorizedResult.Value;
        Assert.NotNull(errorMessage);

        // Verify warning was logged for cross-role attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Non-agent user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AgentLogin_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto(
            "invalid@example.com",
            "wrongpassword");

        var result = Result<AuthenticationResultDto>.Failure("Invalid email or password");
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        var response = await _controller.AgentLogin(loginRequest);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
        var errorMessage = unauthorizedResult.Value;
        Assert.NotNull(errorMessage);

        // Verify warning was logged for failed login
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Agent login failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AgentLogin_WithException_ReturnsInternalServerError()
    {
        var loginRequest = new LoginRequestDto(
            "agent@example.com",
            "password123");

        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ThrowsAsync(new Exception("Database connection failed"));

        var response = await _controller.AgentLogin(loginRequest);

        var statusCodeResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(500, statusCodeResult.StatusCode);

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during agent login")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AgentLogin_WithInvalidModelState_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto(
            "invalid-email", // Invalid email format
            "password123");

        _controller.ModelState.AddModelError("Email", "Invalid email format.");

        var response = await _controller.AgentLogin(loginRequest);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task PlayerLogin_SuccessfulLogin_LogsInformation()
    {
        var loginRequest = new LoginRequestDto(
            "player@example.com",
            "password123");

        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            loginRequest.Email,
            "Test Player",
            "P001");

        var authResult = new AuthenticationResultDto(
            playerDto,
            "jwt.token.here",
            DateTime.UtcNow.AddHours(1));

        var result = Result<AuthenticationResultDto>.Success(authResult);
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        await _controller.PlayerLogin(loginRequest);

        // Verify success was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Player") && v.ToString()!.Contains("logged in successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AgentLogin_SuccessfulLogin_LogsInformation()
    {
        var loginRequest = new LoginRequestDto(
            "agent@example.com",
            "password123");

        var agentDto = new AgentDto(
            Guid.NewGuid(),
            loginRequest.Email,
            "Test Agent");

        var authResult = new AuthenticationResultDto(
            agentDto,
            "jwt.token.here",
            DateTime.UtcNow.AddHours(1));

        var result = Result<AuthenticationResultDto>.Success(authResult);
        _mockAuthenticationService.Setup(s => s.LoginAsync(loginRequest))
                                  .ReturnsAsync(result);

        await _controller.AgentLogin(loginRequest);

        // Verify success was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Agent") && v.ToString()!.Contains("logged in successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
