using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using System.Text.Json;

namespace CustomerServiceApp.Web.State;

/// <summary>
/// Centralized state management store following Redux pattern
/// </summary>
public class AppStateStore
{
    private AppState _state = new();
    private readonly object _lock = new();

    /// <summary>
    /// Event raised when state changes
    /// </summary>
    public event Action? StateChanged;

    /// <summary>
    /// Gets immutable copy of current state
    /// </summary>
    /// <returns>Current application state</returns>
    public AppState GetState()
    {
        lock (_lock)
        {
            return _state with { };
        }
    }

    /// <summary>
    /// Dispatches login action
    /// </summary>
    /// <param name="authResult">Authentication result from login</param>
    public void DispatchLogin(AuthenticationResultDto authResult)
    {
        lock (_lock)
        {
            _state = _state with
            {
                CurrentUser = authResult.User,
                Token = authResult.Token,
                TokenExpiresAt = authResult.ExpiresAt,
                Error = null,
                SuccessMessage = "Successfully signed in"
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches logout action
    /// </summary>
    public void DispatchLogout()
    {
        lock (_lock)
        {
            _state = _state with
            {
                CurrentUser = null,
                Token = null,
                TokenExpiresAt = null,
                Tickets = [],
                SelectedTicket = null,
                Error = null,
                SuccessMessage = "Successfully signed out"
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches loading state change
    /// </summary>
    /// <param name="isLoading">Loading state</param>
    public void DispatchLoadingState(bool isLoading)
    {
        lock (_lock)
        {
            _state = _state with { IsLoading = isLoading };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches error action
    /// </summary>
    /// <param name="error">Error message</param>
    public void DispatchError(string error)
    {
        lock (_lock)
        {
            _state = _state with
            {
                Error = error,
                SuccessMessage = null,
                IsLoading = false
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches success message action
    /// </summary>
    /// <param name="message">Success message</param>
    public void DispatchSuccessMessage(string message)
    {
        lock (_lock)
        {
            _state = _state with
            {
                SuccessMessage = message,
                Error = null
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches tickets loaded action
    /// </summary>
    /// <param name="tickets">List of tickets</param>
    public void DispatchTicketsLoaded(IReadOnlyList<TicketDto> tickets)
    {
        lock (_lock)
        {
            _state = _state with
            {
                Tickets = tickets,
                IsLoading = false,
                Error = null
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches ticket selection action
    /// </summary>
    /// <param name="ticket">Selected ticket</param>
    public void DispatchSelectTicket(TicketDto? ticket)
    {
        lock (_lock)
        {
            _state = _state with { SelectedTicket = ticket };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches ticket creation action
    /// </summary>
    /// <param name="ticket">Newly created ticket</param>
    public void DispatchTicketCreated(TicketDto ticket)
    {
        lock (_lock)
        {
            var updatedTickets = new List<TicketDto>(_state.Tickets) { ticket };
            _state = _state with
            {
                Tickets = updatedTickets.AsReadOnly(),
                SelectedTicket = ticket,
                SuccessMessage = "Ticket created successfully",
                Error = null
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches ticket updated action
    /// </summary>
    /// <param name="updatedTicket">Updated ticket</param>
    public void DispatchTicketUpdated(TicketDto updatedTicket)
    {
        lock (_lock)
        {
            var updatedTickets = _state.Tickets
                .Select(t => t.Id == updatedTicket.Id ? updatedTicket : t)
                .ToList();

            _state = _state with
            {
                Tickets = updatedTickets.AsReadOnly(),
                SelectedTicket = _state.SelectedTicket?.Id == updatedTicket.Id ? updatedTicket : _state.SelectedTicket,
                SuccessMessage = "Ticket updated successfully",
                Error = null
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears all messages
    /// </summary>
    public void DispatchClearMessages()
    {
        lock (_lock)
        {
            _state = _state with
            {
                Error = null,
                SuccessMessage = null
            };
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Notifies all subscribers that state has changed
    /// </summary>
    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
