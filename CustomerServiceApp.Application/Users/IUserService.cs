using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Models;

namespace CustomerServiceApp.Application.Users;

/// <summary>
/// Interface for user service operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new player account
    /// </summary>
    Task<Result<PlayerDto>> CreatePlayerAsync(CreatePlayerDto dto);

    /// <summary>
    /// Creates a new agent account
    /// </summary>
    Task<Result<AgentDto>> CreateAgentAsync(CreateAgentDto dto);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
}
