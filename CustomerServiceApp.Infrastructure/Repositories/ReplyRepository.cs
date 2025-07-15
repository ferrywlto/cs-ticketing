using Microsoft.EntityFrameworkCore;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Infrastructure.Data;

namespace CustomerServiceApp.Infrastructure.Repositories;

/// <summary>
/// Implementation of reply repository using Entity Framework
/// </summary>
public class ReplyRepository : IReplyRepository
{
    private readonly CustomerServiceDbContext _context;

    public ReplyRepository(CustomerServiceDbContext context)
    {
        _context = context;
    }

    public Task<Reply> CreateAsync(Reply reply)
    {
        _context.Replies.Add(reply);
        return Task.FromResult(reply);
    }

    public async Task<Reply?> GetByIdAsync(Guid id)
    {
        return await _context.Replies
            .Include(r => r.Author)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reply>> GetByTicketIdAsync(Guid ticketId)
    {
        return await _context.Replies
            .Include(r => r.Author)
            .Where(r => r.TicketId == ticketId)
            .OrderBy(r => r.CreatedDate)
            .ToListAsync();
    }
}
