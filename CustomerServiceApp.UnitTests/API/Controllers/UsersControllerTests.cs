using CustomerServiceApp.API.Controllers;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Models;
using CustomerServiceApp.Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

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
        var createDto = new CreatePlayerDto(
            "test@example.com",
            "Test Player",
            null,
            "password123",
            "P001");

        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            createDto.Email,
            createDto.Name,
            createDto.PlayerNumber);

        var result = Result<PlayerDto>.Success(playerDto);
        _mockUserService.Setup(s => s.CreatePlayerAsync(createDto))
                       .ReturnsAsync(result);

        var response = await _controller.CreatePlayer(createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var returnedPlayer = Assert.IsType<PlayerDto>(createdResult.Value);
        Assert.Equal(playerDto.Id, returnedPlayer.Id);
        Assert.Equal(playerDto.Email, returnedPlayer.Email);
    }

    [Fact]
    public async Task CreatePlayer_WithFailure_ReturnsBadRequest()
    {
        var createDto = new CreatePlayerDto(
            "test@example.com",
            "Test Player",
            null,
            "password123",
            "P001");

        var result = Result<PlayerDto>.Failure("Email already exists");
        _mockUserService.Setup(s => s.CreatePlayerAsync(createDto))
                       .ReturnsAsync(result);

        var response = await _controller.CreatePlayer(createDto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Email already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateAgent_WithValidDto_ReturnsCreatedResult()
    {
        var createDto = new CreateAgentDto(
            "agent@example.com",
            "Test Agent",
            null,
            "password123");

        var agentDto = new AgentDto(
            Guid.NewGuid(),
            createDto.Email,
            createDto.Name);

        var result = Result<AgentDto>.Success(agentDto);
        _mockUserService.Setup(s => s.CreateAgentAsync(createDto))
                       .ReturnsAsync(result);

        var response = await _controller.CreateAgent(createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var returnedAgent = Assert.IsType<AgentDto>(createdResult.Value);
        Assert.Equal(agentDto.Id, returnedAgent.Id);
        Assert.Equal(agentDto.Email, returnedAgent.Email);
    }

    [Fact]
    public async Task GetUser_WithExistingUser_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var userDto = new PlayerDto(
            userId,
            "test@example.com",
            "Test Player",
            "P001");

        var result = Result<UserDto>.Success(userDto);
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                       .ReturnsAsync(result);

        var response = await _controller.GetUser(userId);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedUser = Assert.IsType<PlayerDto>(okResult.Value);
        Assert.Equal(userId, returnedUser.Id);
    }

    [Fact]
    public async Task GetUser_WithNonExistentUser_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var result = Result<UserDto>.Failure("User not found");
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                       .ReturnsAsync(result);

        var response = await _controller.GetUser(userId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreatePlayer_LogsInformation()
    {
        var createDto = new CreatePlayerDto(
            "test@example.com",
            "Test Player",
            null,
            "password123",
            "P001");

        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            createDto.Email,
            createDto.Name,
            createDto.PlayerNumber);

        var result = Result<PlayerDto>.Success(playerDto);
        _mockUserService.Setup(s => s.CreatePlayerAsync(createDto))
                       .ReturnsAsync(result);

        await _controller.CreatePlayer(createDto);

        // Verify information logging for player creation attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating new player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful creation
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully created player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePlayer_WithFailure_LogsWarning()
    {
        var createDto = new CreatePlayerDto(
            "test@example.com",
            "Test Player",
            null,
            "password123",
            "P001");

        var result = Result<PlayerDto>.Failure("Email already exists");
        _mockUserService.Setup(s => s.CreatePlayerAsync(createDto))
                       .ReturnsAsync(result);

        await _controller.CreatePlayer(createDto);

        // Verify warning logging for failed creation
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAgent_LogsInformation()
    {
        var createDto = new CreateAgentDto(
            "agent@example.com",
            "Test Agent",
            null,
            "password123");

        var agentDto = new AgentDto(
            Guid.NewGuid(),
            createDto.Email,
            createDto.Name);

        var result = Result<AgentDto>.Success(agentDto);
        _mockUserService.Setup(s => s.CreateAgentAsync(createDto))
                       .ReturnsAsync(result);

        await _controller.CreateAgent(createDto);

        // Verify information logging for agent creation attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating new agent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful creation
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully created agent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAgent_WithFailure_LogsWarning()
    {
        var createDto = new CreateAgentDto(
            "agent@example.com",
            "Test Agent",
            null,
            "password123");

        var result = Result<AgentDto>.Failure("Email already exists");
        _mockUserService.Setup(s => s.CreateAgentAsync(createDto))
                       .ReturnsAsync(result);

        var response = await _controller.CreateAgent(createDto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Email already exists", badRequestResult.Value);

        // Verify warning logging for failed creation
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create agent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUser_LogsInformation()
    {
        var userId = Guid.NewGuid();
        var userDto = new PlayerDto(
            userId,
            "test@example.com",
            "Test Player",
            "P001");

        var result = Result<UserDto>.Success(userDto);
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                       .ReturnsAsync(result);

        await _controller.GetUser(userId);

        // Verify information logging for user retrieval attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving user with ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful retrieval
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUser_WithNonExistentUser_LogsWarning()
    {
        var userId = Guid.NewGuid();
        var result = Result<UserDto>.Failure("User not found");
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                       .ReturnsAsync(result);

        await _controller.GetUser(userId);

        // Verify warning logging for user not found
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User") && v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
