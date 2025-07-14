using Microsoft.AspNetCore.Authorization;

namespace CustomerServiceApp.API.Authorization;

/// <summary>
/// Authorization attribute to require player role
/// </summary>
public class RequirePlayerAttribute : AuthorizeAttribute
{
    public RequirePlayerAttribute()
    {
        Roles = "Player";
    }
}

/// <summary>
/// Authorization attribute to require agent role
/// </summary>
public class RequireAgentAttribute : AuthorizeAttribute
{
    public RequireAgentAttribute()
    {
        Roles = "Agent";
    }
}

/// <summary>
/// Authorization attribute to require either player or agent role
/// </summary>
public class RequireUserAttribute : AuthorizeAttribute
{
    public RequireUserAttribute()
    {
        Roles = "Player,Agent";
    }
}
