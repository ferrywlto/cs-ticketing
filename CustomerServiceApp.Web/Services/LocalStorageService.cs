using Microsoft.JSInterop;

namespace CustomerServiceApp.Web.Services;

/// <summary>
/// Service for interacting with browser local storage using JavaScript interop
/// </summary>
public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
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
        catch (JSException)
        {
            // Handle JS exceptions gracefully (e.g., when localStorage is not available)
            return null;
        }
        catch (TaskCanceledException)
        {
            // Handle operation cancellation gracefully
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
        catch (JSException)
        {
            // Handle JS exceptions gracefully (e.g., when localStorage is not available or quota exceeded)
            // Log the error if needed but don't throw to avoid breaking the application
        }
        catch (TaskCanceledException)
        {
            // Handle operation cancellation gracefully
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
        catch (JSException)
        {
            // Handle JS exceptions gracefully
        }
        catch (TaskCanceledException)
        {
            // Handle operation cancellation gracefully
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
        catch (JSException)
        {
            // Handle JS exceptions gracefully
        }
        catch (TaskCanceledException)
        {
            // Handle operation cancellation gracefully
        }
    }
}
