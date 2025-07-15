namespace CustomerServiceApp.Web.Services;

/// <summary>
/// Service for interacting with browser local storage
/// </summary>
public interface ILocalStorageService
{
    /// <summary>
    /// Gets an item from local storage
    /// </summary>
    /// <param name="key">The key of the item to retrieve</param>
    /// <returns>The stored value or null if not found</returns>
    Task<string?> GetItemAsync(string key);

    /// <summary>
    /// Sets an item in local storage
    /// </summary>
    /// <param name="key">The key of the item to store</param>
    /// <param name="value">The value to store</param>
    Task SetItemAsync(string key, string value);

    /// <summary>
    /// Removes an item from local storage
    /// </summary>
    /// <param name="key">The key of the item to remove</param>
    Task RemoveItemAsync(string key);

    /// <summary>
    /// Clears all items from local storage
    /// </summary>
    Task ClearAsync();
}
