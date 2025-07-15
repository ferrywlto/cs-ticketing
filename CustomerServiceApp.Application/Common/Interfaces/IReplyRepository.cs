using CustomerServiceApp.Domain.Tickets;

namespace CustomerServiceApp.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Reply entities
/// </summary>
public interface IReplyRepository
{
    Task<Reply> CreateAsync(Reply reply);
    Task<Reply?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reply>> GetByTicketIdAsync(Guid ticketId);
}
