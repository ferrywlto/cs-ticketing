using System.ComponentModel.DataAnnotations;

namespace CustomerServiceApp.Application.Common.DTOs;

/// <summary>
/// DTO for Reply entities
/// </summary>
public class ReplyDto
{
    public Guid Id { get; init; }
    
    [Required(ErrorMessage = "Content is required.")]
    public required string Content { get; init; }
    
    public required UserDto Author { get; init; }
    
    public DateTime CreatedDate { get; init; }
}

/// <summary>
/// DTO for creating new Reply entities
/// </summary>
public class CreateReplyDto
{
    [Required(ErrorMessage = "Content is required.")]
    public required string Content { get; init; }
    
    [Required(ErrorMessage = "Author ID is required.")]
    public required Guid AuthorId { get; init; }
    
    [Required(ErrorMessage = "Ticket ID is required.")]
    public required Guid TicketId { get; init; }
}
