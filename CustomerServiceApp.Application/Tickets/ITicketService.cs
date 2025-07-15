using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Models;
using CustomerServiceApp.Domain.Tickets;

namespace CustomerServiceApp.Application.Tickets;

/// <summary>
/// Interface for ticket service operations
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Creates a new ticket - only players can create tickets
    /// </summary>
    Task<Result<TicketDto>> CreateTicketAsync(CreateTicketDto dto);

    /// <summary>
    /// Gets a ticket by ID with full details including messages
    /// </summary>
    Task<Result<TicketDto>> GetTicketByIdAsync(Guid ticketId);

    /// <summary>
    /// Gets tickets for a specific player
    /// </summary>
    Task<Result<IEnumerable<TicketSummaryDto>>> GetPlayerTicketsAsync(Guid playerId);

    /// <summary>
    /// Gets tickets by status (for agents)
    /// </summary>
    Task<Result<IEnumerable<TicketSummaryDto>>> GetTicketsByStatusAsync(TicketStatus status);

    /// <summary>
    /// Gets all unresolved tickets (Open and InResolution) for agents
    /// </summary>
    Task<Result<IEnumerable<TicketSummaryDto>>> GetUnresolvedTicketsAsync();

    /// <summary>
    /// Adds a reply to a ticket
    /// </summary>
    Task<Result<ReplyDto>> AddReplyAsync(Guid ticketId, CreateReplyDto dto);

    /// <summary>
    /// Resolves a ticket - only from InResolution status
    /// </summary>
    Task<Result<TicketDto>> ResolveTicketAsync(Guid ticketId, Guid agentId);
}
