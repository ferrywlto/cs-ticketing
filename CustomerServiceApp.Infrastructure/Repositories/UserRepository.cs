using Microsoft.EntityFrameworkCore;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Domain.Users;
using CustomerServiceApp.Infrastructure.Data;

namespace CustomerServiceApp.Infrastructure.Repositories;

/// <summary>
/// Implementation of user repository using Entity Framework
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly CustomerServiceDbContext _context;

    public UserRepository(CustomerServiceDbContext context)
    {
        _context = context;
    }

    public Task<TUser> CreateAsync<TUser>(TUser user) where TUser : User
    {
        _context.Users.Add(user);
        return Task.FromResult(user);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
