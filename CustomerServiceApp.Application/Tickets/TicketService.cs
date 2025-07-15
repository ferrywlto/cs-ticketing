using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Common.Models;
using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Application.Tickets;

/// <summary>
/// Ticket service with exception handling and Result pattern
/// </summary>
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TicketService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new ticket - only players can create tickets
    /// </summary>
    public async Task<Result<TicketDto>> CreateTicketAsync(CreateTicketDto dto)
    {
        try
        {
            var creator = await _unitOfWork.Users.GetByIdAsync(dto.CreatorId);
            if (creator == null)
            {
                return Result<TicketDto>.Failure($"User with ID '{dto.CreatorId}' was not found.");
            }

            if (creator is not Player player)
            {
                return Result<TicketDto>.Failure("Only players can create tickets.");
            }

            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                Creator = player
            };

            await _unitOfWork.Tickets.CreateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            return Result<TicketDto>.Success(_mapper.MapToDto(ticket));
        }
        catch (Exception ex)
        {
            return Result<TicketDto>.Failure($"Failed to create ticket: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a ticket by ID
    /// </summary>
    public async Task<Result<TicketDto>> GetTicketByIdAsync(Guid ticketId)
    {
        try
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return Result<TicketDto>.Failure($"Ticket with ID '{ticketId}' was not found.");
            }

            return Result<TicketDto>.Success(_mapper.MapToDto(ticket));
        }
        catch (Exception ex)
        {
            return Result<TicketDto>.Failure($"Failed to get ticket: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets tickets for a player
    /// </summary>
    public async Task<Result<IEnumerable<TicketSummaryDto>>> GetPlayerTicketsAsync(Guid playerId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(playerId);
            if (user == null)
            {
                return Result<IEnumerable<TicketSummaryDto>>.Failure($"User with ID '{playerId}' was not found.");
            }

            if (user is not Player player)
            {
                return Result<IEnumerable<TicketSummaryDto>>.Failure("Only players can view their tickets.");
            }

            var tickets = await _unitOfWork.Tickets.GetByPlayerAsync(player);
            var summaries = tickets.Select(t => _mapper.MapToSummaryDto(t));

            return Result<IEnumerable<TicketSummaryDto>>.Success(summaries);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TicketSummaryDto>>.Failure($"Failed to get player tickets: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets tickets by status (for agents)
    /// </summary>
    public async Task<Result<IEnumerable<TicketSummaryDto>>> GetTicketsByStatusAsync(TicketStatus status)
    {
        try
        {
            var tickets = await _unitOfWork.Tickets.GetByStatusAsync(status);
            var summaries = tickets.Select(t => _mapper.MapToSummaryDto(t));

            return Result<IEnumerable<TicketSummaryDto>>.Success(summaries);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TicketSummaryDto>>.Failure($"Failed to get tickets by status: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all unresolved tickets (Open and InResolution) for agents
    /// </summary>
    public async Task<Result<IEnumerable<TicketSummaryDto>>> GetUnresolvedTicketsAsync()
    {
        try
        {
            var openTickets = await _unitOfWork.Tickets.GetByStatusAsync(TicketStatus.Open);
            var inResolutionTickets = await _unitOfWork.Tickets.GetByStatusAsync(TicketStatus.InResolution);
            
            var allUnresolvedTickets = openTickets.Concat(inResolutionTickets)
                                                 .OrderByDescending(t => t.LastUpdateDate);
            
            var summaries = allUnresolvedTickets.Select(t => _mapper.MapToSummaryDto(t));

            return Result<IEnumerable<TicketSummaryDto>>.Success(summaries);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TicketSummaryDto>>.Failure($"Failed to get unresolved tickets: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a reply to a ticket
    /// </summary>
    public async Task<Result<ReplyDto>> AddReplyAsync(Guid ticketId, CreateReplyDto dto)
    {
        try
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return Result<ReplyDto>.Failure($"Ticket with ID '{ticketId}' was not found.");
            }

            if (ticket.Status == TicketStatus.Resolved)
            {
                return Result<ReplyDto>.Failure($"Cannot add reply to resolved ticket '{ticketId}'.");
            }

            var author = await _unitOfWork.Users.GetByIdAsync(dto.AuthorId);
            if (author == null)
            {
                return Result<ReplyDto>.Failure($"User with ID '{dto.AuthorId}' was not found.");
            }

            var reply = new Reply
            {
                Content = dto.Content,
                Author = author,
                TicketId = ticketId
            };

            // Execute domain logic (this will change ticket status if needed)
            ticket.AddReply(reply);
            
            // Save changes to both ticket and reply
            await _unitOfWork.Replies.CreateAsync(reply);
            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            return Result<ReplyDto>.Success(_mapper.MapToDto(reply));
        }
        catch (Exception ex)
        {
            return Result<ReplyDto>.Failure($"Failed to add reply: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolves a ticket - only from InResolution status
    /// </summary>
    public async Task<Result<TicketDto>> ResolveTicketAsync(Guid ticketId, Guid agentId)
    {
        try
        {
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return Result<TicketDto>.Failure($"Ticket with ID '{ticketId}' was not found.");
            }

            if (ticket.Status != TicketStatus.InResolution)
            {
                return Result<TicketDto>.Failure($"Can only resolve tickets that are in resolution status. Current status: {ticket.Status}");
            }

            var agent = await _unitOfWork.Users.GetByIdAsync(agentId) as Agent;
            if (agent == null)
            {
                return Result<TicketDto>.Failure($"Agent with ID '{agentId}' was not found.");
            }

            ticket.Resolve(agent);
            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            return Result<TicketDto>.Success(_mapper.MapToDto(ticket));
        }
        catch (Exception ex)
        {
            return Result<TicketDto>.Failure($"Failed to resolve ticket: {ex.Message}");
        }
    }
}