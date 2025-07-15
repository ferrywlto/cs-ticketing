using System.ComponentModel.DataAnnotations;

namespace CustomerServiceApp.Application.Common.DTOs;

/// <summary>
/// DTO record for Reply entities
/// </summary>
public record ReplyDto(
    Guid Id,
    [Required(ErrorMessage = "Content is required.")]
    string Content,
    UserDto Author,
    DateTime CreatedDate = default);

/// <summary>
/// DTO record for creating new Reply entities
/// </summary>
public record CreateReplyDto(
    [Required(ErrorMessage = "Content is required.")]
    string Content,
    [Required(ErrorMessage = "Author ID is required.")]
    Guid AuthorId,
    [Required(ErrorMessage = "Ticket ID is required.")]
    Guid TicketId);
