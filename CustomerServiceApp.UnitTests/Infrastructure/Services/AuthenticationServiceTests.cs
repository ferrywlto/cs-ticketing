using System.IdentityModel.Tokens.Jwt;
using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Domain.Users;
using CustomerServiceApp.Infrastructure.Options;
using CustomerServiceApp.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace CustomerServiceApp.UnitTests.Infrastructure.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _authenticationService = new AuthenticationService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenService.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var player = new Player 
        { 
            Id = Guid.NewGuid(), 
            Email = loginRequest.Email, 
            Name = "Test Player",
            PasswordHash = "hashedPassword",
            PlayerNumber = "P001"
        };

        var playerDto = new PlayerDto
        {
            Id = player.Id,
            Email = loginRequest.Email,
            Name = "Test Player",
            PlayerNumber = "P001"
        };

        var token = "jwt.token.here";
        var mockUserRepository = new Mock<IUserRepository>();
        
        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepository.Object);
        mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email))
                         .ReturnsAsync(player);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(loginRequest.Password, player.PasswordHash))
                          .Returns(true);
        _mockMapper.Setup(m => m.MapToDto(It.IsAny<User>())).Returns(playerDto);
        _mockJwtTokenService.Setup(j => j.GenerateToken(playerDto))
                           .Returns(token);

        var result = await _authenticationService.LoginAsync(loginRequest);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(playerDto.Id, result.Data.User.Id);
        Assert.Equal(token, result.Data.Token);
        Assert.True(result.Data.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsFailure()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepository.Object);
        mockUserRepository.Setup(r => r.GetByEmailAsync(loginRequest.Email))
                         .ReturnsAsync((User?)null);

        var result = await _authenticationService.LoginAsync(loginRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password", result.Error);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsUserDto()
    {
        var userId = Guid.NewGuid();
        var player = new Player
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test Player",
            PasswordHash = "hashedPassword",
            PlayerNumber = "P001"
        };

        var playerDto = new PlayerDto
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test Player",
            PlayerNumber = "P001"
        };

        // Create a real JWT token for testing
        var mockOptions = new Mock<IOptions<JwtTokenOptions>>();
        var jwtTokenOptions = new JwtTokenOptions
        {
            SecretKey = "ThisIsASecretKeyForJWTTokenGeneration32Characters",
            Issuer = "CustomerServiceApp",
            Audience = "CustomerServiceApp.Users",
            ExpiryMinutes = 60
        };
        mockOptions.Setup(o => o.Value).Returns(jwtTokenOptions);
        
        var realJwtService = new JwtTokenService(mockOptions.Object);
        var token = realJwtService.GenerateToken(playerDto);
        var validatedToken = realJwtService.ValidateToken(token);

        var mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork.Setup(u => u.Users).Returns(mockUserRepository.Object);

        _mockJwtTokenService.Setup(j => j.ValidateToken(token))
                           .Returns(validatedToken);
        _mockJwtTokenService.Setup(j => j.GetUserIdFromValidatedToken(validatedToken!))
                           .Returns(userId);
        mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                         .ReturnsAsync(player);
        _mockMapper.Setup(m => m.MapToDto(It.IsAny<User>())).Returns(playerDto);

        var result = await _authenticationService.ValidateTokenAsync(token);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Data!.Id);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFailure()
    {
        var token = "invalid.jwt.token";

        _mockJwtTokenService.Setup(j => j.ValidateToken(token))
                           .Returns((JwtSecurityToken?)null);

        var result = await _authenticationService.ValidateTokenAsync(token);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid token", result.Error);
    }
}
