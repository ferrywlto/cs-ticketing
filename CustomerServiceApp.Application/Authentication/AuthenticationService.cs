using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Common.Models;

namespace CustomerServiceApp.Application.Authentication;

/// <summary>
/// Implementation of authentication service using JWT tokens
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthenticationResultDto>> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            // Authenticate user directly
            var user = await _unitOfWork.Users.GetByEmailAsync(loginRequest.Email);
            if (user == null || !_passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Result<AuthenticationResultDto>.Failure("Invalid email or password");
            }

            var userDto = _mapper.MapToDto(user);
            var token = _jwtTokenService.GenerateToken(userDto);
            var expiresAt = DateTime.UtcNow.AddHours(1); // Default 1 hour expiry

            var authResult = new AuthenticationResultDto(userDto, token, expiresAt);

            return Result<AuthenticationResultDto>.Success(authResult);
        }
        catch (Exception ex)
        {
            return Result<AuthenticationResultDto>.Failure($"Authentication failed: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtToken = _jwtTokenService.ValidateToken(token);
            if (jwtToken == null)
            {
                return Result<UserDto>.Failure("Invalid token");
            }

            var userId = _jwtTokenService.GetUserIdFromValidatedToken(jwtToken);
            if (userId == null)
            {
                return Result<UserDto>.Failure("Invalid token claims");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userDto = _mapper.MapToDto(user);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Token validation failed: {ex.Message}");
        }
    }
}
