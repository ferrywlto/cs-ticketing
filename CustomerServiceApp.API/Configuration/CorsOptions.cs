namespace CustomerServiceApp.API.Configuration;

/// <summary>
/// Configuration settings for Cross-Origin Resource Sharing (CORS)
/// </summary>
public class CorsOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// List of allowed origins for CORS requests
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
