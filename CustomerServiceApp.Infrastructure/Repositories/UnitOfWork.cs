using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Infrastructure.Data;

namespace CustomerServiceApp.Infrastructure.Repositories;

/// <summary>
/// Implementation of Unit of Work pattern using Entity Framework
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CustomerServiceDbContext _context;
    private ITicketRepository? _tickets;
    private IUserRepository? _users;
    private IReplyRepository? _replies;

    public UnitOfWork(CustomerServiceDbContext context)
    {
        _context = context;
    }

    public ITicketRepository Tickets => _tickets ??= new TicketRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IReplyRepository Replies => _replies ??= new ReplyRepository(_context);

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
