namespace CustomerServiceApp.Web.Configuration;

/// <summary>
/// Configuration settings for API endpoints
/// </summary>
public class ApiSettings
{
    public const string SectionName = "ApiSettings";
    
    /// <summary>
    /// Base URL for the API server
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
}
