using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CustomerServiceApp.Web.Services;

/// <summary>
/// Service for interacting with browser local storage using JavaScript interop
/// </summary>
public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IJSRuntime jsRuntime, ILogger<LocalStorageService> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string?> GetItemAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (JSException ex)
        {
            // Handle JS exceptions gracefully (e.g., when localStorage is not available)
            _logger.LogWarning(ex, "Failed to get item from localStorage with key '{Key}'", key);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            // Handle operation cancellation gracefully
            _logger.LogWarning(ex, "Operation cancelled while getting item from localStorage with key '{Key}'", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetItemAsync(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (JSException ex)
        {
            // Handle JS exceptions gracefully (e.g., when localStorage is not available or quota exceeded)
            _logger.LogWarning(ex, "Failed to set item in localStorage with key '{Key}' and value length {ValueLength}", key, value.Length);
        }
        catch (TaskCanceledException ex)
        {
            // Handle operation cancellation gracefully
            _logger.LogWarning(ex, "Operation cancelled while setting item in localStorage with key '{Key}'", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveItemAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (JSException ex)
        {
            // Handle JS exceptions gracefully
            _logger.LogWarning(ex, "Failed to remove item from localStorage with key '{Key}'", key);
        }
        catch (TaskCanceledException ex)
        {
            // Handle operation cancellation gracefully
            _logger.LogWarning(ex, "Operation cancelled while removing item from localStorage with key '{Key}'", key);
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
        catch (JSException ex)
        {
            // Handle JS exceptions gracefully
            _logger.LogWarning(ex, "Failed to clear localStorage");
        }
        catch (TaskCanceledException ex)
        {
            // Handle operation cancellation gracefully
            _logger.LogWarning(ex, "Operation cancelled while clearing localStorage");
        }
    }
}
