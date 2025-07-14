using CustomerServiceApp.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerServiceApp.API.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a player and returns a JWT token
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>Authentication result with JWT token for players only</returns>
    [HttpPost("player/login")]
    [AllowAnonymous]
    public async Task<IActionResult> PlayerLogin([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authenticationService.LoginAsync(loginRequest);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Player login failed for email: {Email}. Error: {Error}", 
                    loginRequest.Email, result.Error);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Check if the authenticated user is actually a player
            if (result.Data!.User.UserType != "Player")
            {
                _logger.LogWarning("Non-player user {Email} attempted to login via player endpoint", 
                    loginRequest.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            _logger.LogInformation("Player {Email} logged in successfully", loginRequest.Email);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during player login for email: {Email}", loginRequest.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Authenticates an agent and returns a JWT token
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>Authentication result with JWT token for agents only</returns>
    [HttpPost("agent/login")]
    [AllowAnonymous]
    public async Task<IActionResult> AgentLogin([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authenticationService.LoginAsync(loginRequest);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Agent login failed for email: {Email}. Error: {Error}", 
                    loginRequest.Email, result.Error);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Check if the authenticated user is actually an agent
            if (result.Data!.User.UserType != "Agent")
            {
                _logger.LogWarning("Non-agent user {Email} attempted to login via agent endpoint", 
                    loginRequest.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            _logger.LogInformation("Agent {Email} logged in successfully", loginRequest.Email);
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during agent login for email: {Email}", loginRequest.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }
}
