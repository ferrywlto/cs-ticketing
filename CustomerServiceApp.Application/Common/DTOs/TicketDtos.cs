using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Domain.Tickets;

namespace CustomerServiceApp.Application.Common.DTOs;

/// <summary>
/// DTO for Ticket entities
/// </summary>
public class TicketDto
{
    public Guid Id { get; init; }
    
    [Required(ErrorMessage = "Title is required.")]
    public required string Title { get; init; }
    
    [Required(ErrorMessage = "Description is required.")]
    public required string Description { get; init; }
    
    public required PlayerDto Creator { get; init; }
    
    public string Status { get; init; } = "Open";
    
    public DateTime CreatedDate { get; init; }
    
    public DateTime LastUpdateDate { get; init; }
    
    public DateTime? ResolvedDate { get; init; }
    
    public IEnumerable<ReplyDto> Messages { get; init; } = new List<ReplyDto>();
}

/// <summary>
/// DTO for creating new Ticket entities
/// </summary>
public class CreateTicketDto
{
    [Required(ErrorMessage = "Title is required.")]
    public required string Title { get; init; }
    
    [Required(ErrorMessage = "Description is required.")]
    public required string Description { get; init; }
    
    [Required(ErrorMessage = "Creator ID is required.")]
    public required Guid CreatorId { get; init; }
}

/// <summary>
/// DTO for ticket list views with minimal information
/// </summary>
public class TicketSummaryDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required PlayerDto Creator { get; init; }
    public string Status { get; init; } = "Open";
    public DateTime CreatedDate { get; init; }
    public DateTime LastUpdateDate { get; init; }
    public int MessageCount { get; init; }
}

/// <summary>
/// DTO for updating ticket status
/// </summary>
public class UpdateTicketStatusDto
{
    [Required(ErrorMessage = "Ticket ID is required.")]
    public required Guid TicketId { get; init; }
    
    [Required(ErrorMessage = "Status is required.")]
    public required string Status { get; init; }
}

/// <summary>
/// DTO for updating ticket information
/// </summary>
public class UpdateTicketDto
{
    /// <summary>
    /// The ticket's title
    /// </summary>
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string? Title { get; set; }

    /// <summary>
    /// The ticket's description
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// The ticket's status
    /// </summary>
    public TicketStatus? Status { get; set; }
}
