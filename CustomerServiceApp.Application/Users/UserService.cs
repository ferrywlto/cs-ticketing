using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Common.Models;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Application.Users;

/// <summary>
/// User service with exception handling and Result pattern
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Creates a new player account
    /// </summary>
    public async Task<Result<PlayerDto>> CreatePlayerAsync(CreatePlayerDto dto)
    {
        try
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return Result<PlayerDto>.Failure($"User with email '{dto.Email}' already exists.");
            }

            var player = new Player
            {
                Email = dto.Email,
                Name = dto.Name,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                PlayerNumber = dto.PlayerNumber
            };

            await _unitOfWork.Users.CreateAsync(player);
            await _unitOfWork.SaveChangesAsync();

            return Result<PlayerDto>.Success(_mapper.MapToDto(player));
        }
        catch (Exception ex)
        {
            return Result<PlayerDto>.Failure($"Failed to create player: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new agent account
    /// </summary>
    public async Task<Result<AgentDto>> CreateAgentAsync(CreateAgentDto dto)
    {
        try
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return Result<AgentDto>.Failure($"User with email '{dto.Email}' already exists.");
            }

            var agent = new Agent
            {
                Email = dto.Email,
                Name = dto.Name,
                PasswordHash = _passwordHasher.HashPassword(dto.Password)
            };

            await _unitOfWork.Users.CreateAsync(agent);
            await _unitOfWork.SaveChangesAsync();

            return Result<AgentDto>.Success(_mapper.MapToDto(agent));
        }
        catch (Exception ex)
        {
            return Result<AgentDto>.Failure($"Failed to create agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDto>.Failure($"User with ID '{userId}' was not found.");
            }

            return Result<UserDto>.Success(_mapper.MapToDto(user));
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to get user: {ex.Message}");
        }
    }
}