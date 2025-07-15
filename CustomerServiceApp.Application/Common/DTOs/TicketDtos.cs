using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Domain.Tickets;

namespace CustomerServiceApp.Application.Common.DTOs;

/// <summary>
/// DTO record for Ticket entities
/// </summary>
public record TicketDto(
    Guid Id,
    [property: Required(ErrorMessage = "Title is required.")]
    string Title,
    [property: Required(ErrorMessage = "Description is required.")]
    string Description,
    PlayerDto Creator,
    string Status = "Open",
    DateTime CreatedDate = default,
    DateTime LastUpdateDate = default,
    DateTime? ResolvedDate = null,
    IEnumerable<ReplyDto>? Messages = null)
{
    public IEnumerable<ReplyDto> Messages { get; init; } = Messages ?? new List<ReplyDto>();
}

/// <summary>
/// DTO record for creating new Ticket entities
/// </summary>
public record CreateTicketDto(
    [property: Required(ErrorMessage = "Title is required.")]
    string Title,
    [property: Required(ErrorMessage = "Description is required.")]
    string Description,
    [property: Required(ErrorMessage = "Creator ID is required.")]
    Guid CreatorId);

/// <summary>
/// DTO record for ticket list views with minimal information
/// </summary>
public record TicketSummaryDto(
    Guid Id,
    string Title,
    string Description,
    PlayerDto Creator,
    string Status = "Open",
    DateTime CreatedDate = default,
    DateTime LastUpdateDate = default,
    int MessageCount = 0);

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
