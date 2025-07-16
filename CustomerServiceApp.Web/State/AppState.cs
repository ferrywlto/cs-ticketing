using CustomerServiceApp.Application.Common.DTOs;

namespace CustomerServiceApp.Web.State;

/// <summary>
/// Immutable application state record following Redux pattern
/// </summary>
public record AppState
{
    /// <summary>
    /// Current authenticated user, null if not authenticated
    /// </summary>
    public UserDto? CurrentUser { get; init; }

    /// <summary>
    /// JWT token for authentication, null if not authenticated
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? TokenExpiresAt { get; init; }

    /// <summary>
    /// List of tickets visible to the current user
    /// </summary>
    public IReadOnlyList<TicketDto> Tickets { get; init; } = [];

    /// <summary>
    /// Currently selected ticket for detail view
    /// </summary>
    public TicketDto? SelectedTicket { get; init; }

    /// <summary>
    /// Loading state for async operations
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Error message for display
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Success message for display
    /// </summary>
    public string? SuccessMessage { get; init; }

    /// <summary>
    /// Indicates if user is authenticated
    /// </summary>
    public bool IsAuthenticated => CurrentUser is not null && !string.IsNullOrEmpty(Token);

    /// <summary>
    /// Indicates if current user is a player
    /// </summary>
    public bool IsPlayer => CurrentUser is PlayerDto;

    /// <summary>
    /// Indicates if current user is an agent
    /// </summary>
    public bool IsAgent => CurrentUser is AgentDto;
}
