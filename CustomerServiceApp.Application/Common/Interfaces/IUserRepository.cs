using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Application.Common.Interfaces;

/// <summary>
/// Repository interface for User entities
/// </summary>
public interface IUserRepository
{
    Task<TUser> CreateAsync<TUser>(TUser user) where TUser : User;
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
}