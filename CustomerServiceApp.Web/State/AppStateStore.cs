using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Web.Services;
using System.Text.Json;

namespace CustomerServiceApp.Web.State;

/// <summary>
/// Centralized state management store following Redux pattern with local storage persistence
/// </summary>
public class AppStateStore
{
    private AppState _state = new();
    private readonly object _lock = new();
    private readonly ILocalStorageService _localStorageService;
    private readonly ILogger<AppStateStore> _logger;
    private const string StorageKey = "app-state";

    public AppStateStore(ILocalStorageService localStorageService, ILogger<AppStateStore> logger)
    {
        _localStorageService = localStorageService;
        _logger = logger;
    }

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
    public async Task DispatchLoginAsync(AuthenticationResultDto authResult)
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
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches logout action
    /// </summary>
    public async Task DispatchLogoutAsync()
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
        await ClearPersistedStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches loading state change
    /// </summary>
    /// <param name="isLoading">Loading state</param>
    public void DispatchLoadingStateAsync(bool isLoading)
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
    public async Task DispatchErrorAsync(string error)
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
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches success message action
    /// </summary>
    /// <param name="message">Success message</param>
    public async Task DispatchSuccessMessageAsync(string message)
    {
        lock (_lock)
        {
            _state = _state with
            {
                SuccessMessage = message,
                Error = null
            };
        }
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches tickets loaded action
    /// </summary>
    /// <param name="tickets">List of tickets</param>
    public async Task DispatchTicketsLoadedAsync(IReadOnlyList<TicketDto> tickets)
    {
        lock (_lock)
        {
            // Sort tickets by last update date in descending order (most recent first)
            var sortedTickets = tickets.OrderByDescending(t => t.LastUpdateDate).ToList().AsReadOnly();
            
            _state = _state with
            {
                Tickets = sortedTickets,
                IsLoading = false,
                Error = null
            };
        }
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches ticket selection action
    /// </summary>
    /// <param name="ticket">Selected ticket</param>
    public async Task DispatchSelectTicketAsync(TicketDto? ticket)
    {
        lock (_lock)
        {
            _state = _state with { SelectedTicket = ticket };
        }
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches ticket creation action
    /// </summary>
    /// <param name="ticket">Newly created ticket</param>
    public async Task DispatchTicketCreatedAsync(TicketDto ticket)
    {
        lock (_lock)
        {
            var updatedTickets = new List<TicketDto>(_state.Tickets) { ticket };
            // Sort tickets by last update date in descending order (most recent first)
            var sortedTickets = updatedTickets.OrderByDescending(t => t.LastUpdateDate).ToList().AsReadOnly();
            
            _state = _state with
            {
                Tickets = sortedTickets,
                SelectedTicket = ticket,
                SuccessMessage = "Ticket created successfully",
                Error = null
            };
        }
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Dispatches ticket updated action
    /// </summary>
    /// <param name="updatedTicket">Updated ticket</param>
    public async Task DispatchTicketUpdatedAsync(TicketDto updatedTicket)
    {
        lock (_lock)
        {
            var updatedTickets = _state.Tickets
                .Select(t => t.Id == updatedTicket.Id ? updatedTicket : t)
                .ToList();

            // Sort tickets by last update date in descending order (most recent first)
            var sortedTickets = updatedTickets.OrderByDescending(t => t.LastUpdateDate).ToList().AsReadOnly();

            _state = _state with
            {
                Tickets = sortedTickets,
                SelectedTicket = _state.SelectedTicket?.Id == updatedTicket.Id ? updatedTicket : _state.SelectedTicket,
                SuccessMessage = "Ticket updated successfully",
                Error = null
            };
        }
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears all messages
    /// </summary>
    public async Task DispatchClearMessagesAsync()
    {
        lock (_lock)
        {
            _state = _state with
            {
                Error = null,
                SuccessMessage = null
            };
        }
        await PersistStateAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// Notifies all subscribers that state has changed
    /// </summary>
    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Loads state from local storage if available
    /// </summary>
    public async Task LoadStateFromLocalStorageAsync()
    {
        if (_localStorageService is null)
            return;

        try
        {
            var storedState = await _localStorageService.GetItemAsync(StorageKey);
            if (string.IsNullOrEmpty(storedState))
                return;

            var deserializedState = JsonSerializer.Deserialize<AppState>(storedState);
            if (deserializedState is null)
                return;

            // Check if token is expired
            if (deserializedState.TokenExpiresAt.HasValue && 
                deserializedState.TokenExpiresAt.Value <= DateTime.UtcNow)
            {
                // Token expired, clear storage and return
                await ClearPersistedStateAsync();
                return;
            }

            lock (_lock)
            {
                _state = deserializedState;
            }
            NotifyStateChanged();
        }
        catch (JsonException ex)
        {
            // Invalid JSON in storage, ignore and continue with default state
            _logger?.LogWarning(ex, "Failed to deserialize state from local storage. Invalid JSON format detected. Clearing corrupted state.");
            await ClearPersistedStateAsync();
        }
        catch (Exception ex)
        {
            // Any other exception, ignore and continue with default state
            _logger?.LogWarning(ex, "Unexpected error occurred while loading state from local storage. Continuing with default state.");
        }
    }

    /// <summary>
    /// Persists current state to local storage
    /// </summary>
    private async Task PersistStateAsync()
    {
        if (_localStorageService is null)
            return;

        try
        {
            // Only persist authentication-related state
            var stateToPersist = new AppState
            {
                CurrentUser = _state.CurrentUser,
                Token = _state.Token,
                TokenExpiresAt = _state.TokenExpiresAt
            };

            var serializedState = JsonSerializer.Serialize(stateToPersist);
            await _localStorageService.SetItemAsync(StorageKey, serializedState);
        }
        catch (Exception ex)
        {
            // Fail silently to avoid breaking the application but log the error
            _logger?.LogWarning(ex, "Failed to persist state to local storage. Storage may be unavailable or quota exceeded.");
        }
    }

    /// <summary>
    /// Clears persisted state from local storage
    /// </summary>
    private async Task ClearPersistedStateAsync()
    {
        if (_localStorageService is null)
            return;

        try
        {
            await _localStorageService.RemoveItemAsync(StorageKey);
        }
        catch (Exception ex)
        {
            // Fail silently to avoid breaking the application but log the error
            _logger?.LogWarning(ex, "Failed to clear persisted state from local storage. Storage may be unavailable.");
        }
    }
}
