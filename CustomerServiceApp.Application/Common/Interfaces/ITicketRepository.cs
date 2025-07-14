using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Ticket entities
/// </summary>
public interface ITicketRepository
{
    Task<Ticket> CreateAsync(Ticket ticket);
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<IEnumerable<Ticket>> GetByPlayerAsync(Player player);
    Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status);
    Task UpdateAsync(Ticket ticket);
}