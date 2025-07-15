using Microsoft.EntityFrameworkCore;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;
using CustomerServiceApp.Infrastructure.Data;

namespace CustomerServiceApp.Infrastructure.Repositories;

/// <summary>
/// Implementation of ticket repository using Entity Framework
/// </summary>
public class TicketRepository : ITicketRepository
{
    private readonly CustomerServiceDbContext _context;

    public TicketRepository(CustomerServiceDbContext context)
    {
        _context = context;
    }

    public Task<Ticket> CreateAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        return Task.FromResult(ticket);
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .Include(t => t.Creator)
            .Include(t => t.Messages)
                .ThenInclude(m => m.Author)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Ticket>> GetByPlayerAsync(Player player)
    {
        return await _context.Tickets
            .Include(t => t.Creator)
            .Include(t => t.Messages)
            .Where(t => t.Creator.Id == player.Id)
            .OrderByDescending(t => t.LastUpdateDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status)
    {
        return await _context.Tickets
            .Include(t => t.Creator)
            .Include(t => t.Messages)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.LastUpdateDate)
            .ToListAsync();
    }

    public Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        return Task.CompletedTask;
    }
}
