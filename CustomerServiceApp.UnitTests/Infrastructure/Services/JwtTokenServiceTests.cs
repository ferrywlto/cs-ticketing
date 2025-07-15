using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Infrastructure.Options;
using CustomerServiceApp.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace CustomerServiceApp.UnitTests.Infrastructure.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<IOptions<JwtTokenOptions>> _mockOptions;
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        _mockOptions = new Mock<IOptions<JwtTokenOptions>>();
        
        // Setup JWT configuration
        var jwtTokenOptions = new JwtTokenOptions
        {
            SecretKey = "ThisIsASecretKeyForJWTTokenGeneration32Characters",
            Issuer = "CustomerServiceApp",
            Audience = "CustomerServiceApp.Users",
            ExpiryMinutes = 60
        };
        
        _mockOptions.Setup(o => o.Value).Returns(jwtTokenOptions);
        _jwtTokenService = new JwtTokenService(_mockOptions.Object);
    }

    [Fact]
    public void GenerateToken_WithValidPlayerDto_ReturnsValidJwtToken()
    {
        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            "test@example.com",
            "Test Player",
            "P001");

        var token = _jwtTokenService.GenerateToken(playerDto);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void GenerateToken_WithValidAgentDto_ReturnsValidJwtToken()
    {
        var agentDto = new AgentDto(
            Guid.NewGuid(),
            "agent@example.com",
            "Test Agent");

        var token = _jwtTokenService.GenerateToken(agentDto);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsJwtSecurityToken()
    {
        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            "test@example.com",
            "Test Player",
            "P001");

        var token = _jwtTokenService.GenerateToken(playerDto);
        var jwtToken = _jwtTokenService.ValidateToken(token);

        Assert.NotNull(jwtToken);
        var userId = _jwtTokenService.GetUserIdFromValidatedToken(jwtToken);
        Assert.Equal(playerDto.Id, userId);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        var invalidToken = "invalid.token.here";

        var jwtToken = _jwtTokenService.ValidateToken(invalidToken);

        Assert.Null(jwtToken);
    }

    [Fact]
    public void GetUserIdFromValidatedToken_WithValidJwtToken_ReturnsUserId()
    {
        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            "test@example.com",
            "Test Player",
            "P001");

        var token = _jwtTokenService.GenerateToken(playerDto);
        var jwtToken = _jwtTokenService.ValidateToken(token);
        var userId = _jwtTokenService.GetUserIdFromValidatedToken(jwtToken!);

        Assert.NotNull(userId);
        Assert.Equal(playerDto.Id, userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ReturnsUserId()
    {
        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            "test@example.com",
            "Test Player",
            "P001");

        var token = _jwtTokenService.GenerateToken(playerDto);
        var userId = _jwtTokenService.GetUserIdFromToken(token);

        Assert.NotNull(userId);
        Assert.Equal(playerDto.Id, userId);
    }

    [Fact]
    public void GetExpirationFromValidatedToken_WithValidJwtToken_ReturnsExpirationTime()
    {
        var playerDto = new PlayerDto(
            Guid.NewGuid(),
            "test@example.com",
            "Test Player",
            "P001");

        var beforeGeneration = DateTime.UtcNow;
        var token = _jwtTokenService.GenerateToken(playerDto);
        var afterGeneration = DateTime.UtcNow;
        var jwtToken = _jwtTokenService.ValidateToken(token);
        var expirationTime = _jwtTokenService.GetExpirationFromValidatedToken(jwtToken!);

        Assert.NotNull(expirationTime);
        
        // The expiration should be approximately 60 minutes from generation time
        var expectedExpiration = beforeGeneration.AddMinutes(60);
        var timeDifference = Math.Abs((expirationTime.Value - expectedExpiration).TotalMinutes);
        
        Assert.True(timeDifference < 1, $"Expected expiration to be around 60 minutes from generation. Difference: {timeDifference} minutes");
        Assert.True(expirationTime > afterGeneration, "Expiration time should be in the future");
    }
}
