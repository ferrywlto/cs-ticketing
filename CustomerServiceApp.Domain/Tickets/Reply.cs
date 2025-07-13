using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Domain.Tickets;

public class Reply
{
    public Guid Id { get; init; } = Guid.NewGuid();

    [Required(ErrorMessage = "Content is required.")]
    public required string Content { get; init; }

    [Required(ErrorMessage = "Author is required.")]
    public required User Author { get; init; }

    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}
