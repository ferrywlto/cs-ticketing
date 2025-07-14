using CustomerServiceApp.API.Controllers;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CustomerServiceApp.UnitTests.API.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockUserService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreatePlayer_WithValidDto_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreatePlayerDto
        {
            Email = "test@example.com",
            Name = "Test Player",
            Password = "password123",
            PlayerNumber = "P001"
        };

        var playerDto = new PlayerDto
        {
            Id = Guid.NewGuid(),
            Email = createDto.Email,
            Name = createDto.Name,
            PlayerNumber = createDto.PlayerNumber
        };

        var result = Result<PlayerDto>.Success(playerDto);
        _mockUserService.Setup(s => s.CreatePlayerAsync(createDto))
                       .ReturnsAsync(result);

        // Act
        var response = await _controller.CreatePlayer(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var returnedPlayer = Assert.IsType<PlayerDto>(createdResult.Value);
        Assert.Equal(playerDto.Id, returnedPlayer.Id);
        Assert.Equal(playerDto.Email, returnedPlayer.Email);
    }

    [Fact]
    public async Task CreatePlayer_WithFailure_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreatePlayerDto
        {
            Email = "test@example.com",
            Name = "Test Player",
            Password = "password123",
            PlayerNumber = "P001"
        };

        var result = Result<PlayerDto>.Failure("Email already exists");
        _mockUserService.Setup(s => s.CreatePlayerAsync(createDto))
                       .ReturnsAsync(result);

        // Act
        var response = await _controller.CreatePlayer(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Email already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateAgent_WithValidDto_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateAgentDto
        {
            Email = "agent@example.com",
            Name = "Test Agent",
            Password = "password123"
        };

        var agentDto = new AgentDto
        {
            Id = Guid.NewGuid(),
            Email = createDto.Email,
            Name = createDto.Name
        };

        var result = Result<AgentDto>.Success(agentDto);
        _mockUserService.Setup(s => s.CreateAgentAsync(createDto))
                       .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateAgent(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var returnedAgent = Assert.IsType<AgentDto>(createdResult.Value);
        Assert.Equal(agentDto.Id, returnedAgent.Id);
        Assert.Equal(agentDto.Email, returnedAgent.Email);
    }

    [Fact]
    public async Task GetUser_WithExistingUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new PlayerDto
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test Player",
            PlayerNumber = "P001"
        };

        var result = Result<UserDto>.Success(userDto);
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                       .ReturnsAsync(result);

        // Act
        var response = await _controller.GetUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedUser = Assert.IsType<PlayerDto>(okResult.Value);
        Assert.Equal(userId, returnedUser.Id);
    }

    [Fact]
    public async Task GetUser_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var result = Result<UserDto>.Failure("User not found");
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                       .ReturnsAsync(result);

        // Act
        var response = await _controller.GetUser(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task Authenticate_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var userDto = new PlayerDto
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "Test Player",
            PlayerNumber = "P001"
        };

        var result = Result<UserDto>.Success(userDto);
        _mockUserService.Setup(s => s.AuthenticateAsync(email, password))
                       .ReturnsAsync(result);

        // Act
        var response = await _controller.Authenticate(email, password);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedUser = Assert.IsType<PlayerDto>(okResult.Value);
        Assert.Equal(userDto.Id, returnedUser.Id);
    }

    [Fact]
    public async Task Authenticate_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrongpassword";
        var result = Result<UserDto>.Failure("Invalid credentials");
        _mockUserService.Setup(s => s.AuthenticateAsync(email, password))
                       .ReturnsAsync(result);

        // Act
        var response = await _controller.Authenticate(email, password);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response.Result);
        Assert.Equal("Invalid credentials", unauthorizedResult.Value);
    }
}
