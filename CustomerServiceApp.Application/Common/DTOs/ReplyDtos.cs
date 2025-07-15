using System.ComponentModel.DataAnnotations;

namespace CustomerServiceApp.Application.Common.DTOs;

/// <summary>
/// DTO record for Reply entities
/// </summary>
public record ReplyDto(
    Guid Id,
    [property: Required(ErrorMessage = "Content is required.")]
    string Content,
    UserDto Author,
    DateTime CreatedDate = default);

/// <summary>
/// DTO record for creating new Reply entities
/// </summary>
public record CreateReplyDto(
    [property: Required(ErrorMessage = "Content is required.")]
    string Content,
    [property: Required(ErrorMessage = "Author ID is required.")]
    Guid AuthorId,
    [property: Required(ErrorMessage = "Ticket ID is required.")]
    Guid TicketId);
