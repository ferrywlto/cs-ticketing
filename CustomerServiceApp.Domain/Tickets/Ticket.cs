using System.ComponentModel.DataAnnotations;
using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.Domain.Tickets;

public class Ticket
{
    private readonly List<Reply> _messages = new();

    public Guid Id { get; init; } = Guid.NewGuid();

    [Required(ErrorMessage = "Title is required.")]
    public required string Title { get; init; }

    [Required(ErrorMessage = "Description is required.")]
    public required string Description { get; init; }

    [Required(ErrorMessage = "Creator is required.")]
    public required Player Creator { get; init; }

    public TicketStatus Status { get; private set; } = TicketStatus.Open;

    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public DateTime LastUpdateDate { get; private set; } = DateTime.UtcNow;

    public DateTime? ResolvedDate { get; private set; }

    public IReadOnlyList<Reply> Messages => _messages.OrderBy(m => m.CreatedDate).ToList();

    public void AddReply(Reply reply)
    {
        if (reply == null)
            throw new ArgumentNullException(nameof(reply));

        if (Status == TicketStatus.Resolved)
            throw new InvalidOperationException("Cannot add replies to a resolved ticket.");

        _messages.Add(reply);
        LastUpdateDate = DateTime.UtcNow;

        // If an agent replies and the ticket is Open, change status to InResolution
        if (reply.Author is Agent && Status == TicketStatus.Open)
        {
            Status = TicketStatus.InResolution;
        }
    }

    public void Resolve()
    {
        if (Status != TicketStatus.InResolution)
            throw new InvalidOperationException("Ticket can only be resolved from InResolution status.");

        Status = TicketStatus.Resolved;
        ResolvedDate = DateTime.UtcNow;
        LastUpdateDate = DateTime.UtcNow;
    }
}
