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
        return new PlayerDto
        {
            Id = player.Id,
            Email = player.Email,
            Name = player.Name,
            Avatar = player.Avatar,
            PlayerNumber = player.PlayerNumber
        };
    }

    public AgentDto MapToDto(Agent agent)
    {
        return new AgentDto
        {
            Id = agent.Id,
            Email = agent.Email,
            Name = agent.Name,
            Avatar = agent.Avatar
        };
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
        return new TicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Creator = MapToDto(ticket.Creator),
            Status = ticket.Status.ToString(),
            CreatedDate = ticket.CreatedDate,
            LastUpdateDate = ticket.LastUpdateDate,
            ResolvedDate = ticket.ResolvedDate,
            Messages = ticket.Messages.Select(MapToDto).ToList()
        };
    }

    public TicketSummaryDto MapToSummaryDto(Ticket ticket)
    {
        return new TicketSummaryDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description.Length > 100 
                ? ticket.Description[..100] + "..." 
                : ticket.Description,
            Creator = MapToDto(ticket.Creator),
            Status = ticket.Status.ToString(),
            CreatedDate = ticket.CreatedDate,
            LastUpdateDate = ticket.LastUpdateDate,
            MessageCount = ticket.Messages.Count
        };
    }

    // Reply mappings
    public ReplyDto MapToDto(Reply reply)
    {
        return new ReplyDto
        {
            Id = reply.Id,
            Content = reply.Content,
            Author = MapToDto(reply.Author),
            CreatedDate = reply.CreatedDate
        };
    }
}
