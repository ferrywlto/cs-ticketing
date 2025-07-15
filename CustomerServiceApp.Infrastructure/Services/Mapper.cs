using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Domain.Tickets;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Infrastructure.Services;

/// <summary>
/// Implementation of mapping between domain entities and DTOs
/// </summary>
public class Mapper : IMapper
{
    // User mappings
    public UserDto MapToDto(User user)
    {
        return user switch
        {
            Player player => MapToDto(player),
            Agent agent => MapToDto(agent),
            _ => throw new ArgumentException($"Unknown user type: {user.GetType().Name}")
        };
    }

    public PlayerDto MapToDto(Player player)
    {
        return new PlayerDto(
            player.Id,
            player.Email,
            player.Name,
            player.PlayerNumber,
            player.Avatar);
    }

    public AgentDto MapToDto(Agent agent)
    {
        return new AgentDto(
            agent.Id,
            agent.Email,
            agent.Name,
            agent.Avatar);
    }

    public Player MapToDomain(CreatePlayerDto dto)
    {
        return new Player
        {
            Email = dto.Email,
            Name = dto.Name,
            Avatar = dto.Avatar,
            PasswordHash = dto.Password, // Should be hashed before calling this
            PlayerNumber = dto.PlayerNumber
        };
    }

    public Agent MapToDomain(CreateAgentDto dto)
    {
        return new Agent
        {
            Email = dto.Email,
            Name = dto.Name,
            Avatar = dto.Avatar,
            PasswordHash = dto.Password // Should be hashed before calling this
        };
    }

    // Ticket mappings
    public TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            MapToDto(ticket.Creator),
            ticket.Status.ToString(),
            ticket.CreatedDate,
            ticket.LastUpdateDate,
            ticket.ResolvedDate,
            ticket.ResolvedBy != null ? MapToDto(ticket.ResolvedBy) : null,
            ticket.Messages.Select(MapToDto).ToList());
    }

    public TicketSummaryDto MapToSummaryDto(Ticket ticket)
    {
        return new TicketSummaryDto(
            ticket.Id,
            ticket.Title,
            ticket.Description.Length > 100 
                ? ticket.Description[..100] + "..." 
                : ticket.Description,
            MapToDto(ticket.Creator),
            ticket.Status.ToString(),
            ticket.CreatedDate,
            ticket.LastUpdateDate,
            ticket.Messages.Count);
    }

    // Reply mappings
    public ReplyDto MapToDto(Reply reply)
    {
        return new ReplyDto(
            reply.Id,
            reply.Content,
            MapToDto(reply.Author),
            reply.CreatedDate);
    }
}
