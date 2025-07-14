using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Application.Common.Interfaces;

/// <summary>
/// Interface for mapping between domain entities and DTOs
/// </summary>
public interface IMapper
{
    // User mappings
    UserDto MapToDto(User user);
    PlayerDto MapToDto(Player player);
    AgentDto MapToDto(Agent agent);
    
    Player MapToDomain(CreatePlayerDto dto);
    Agent MapToDomain(CreateAgentDto dto);
    
    // Ticket mappings
    TicketDto MapToDto(Ticket ticket);
    TicketSummaryDto MapToSummaryDto(Ticket ticket);
    
    // Reply mappings
    ReplyDto MapToDto(Reply reply);
}
