namespace CustomerServiceApp.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions
/// </summary>
public interface IUnitOfWork
{
    ITicketRepository Tickets { get; }
    IUserRepository Users { get; }
    IReplyRepository Replies { get; }
    Task SaveChangesAsync();
}