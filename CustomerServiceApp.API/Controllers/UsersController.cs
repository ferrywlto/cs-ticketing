using CustomerServiceApp.API.Authorization;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace CustomerServiceApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("players")]
    [RequireAgent]
    public async Task<ActionResult<PlayerDto>> CreatePlayer(CreatePlayerDto dto)
    {
        _logger.LogInformation("Creating new player with email: {Email}", dto.Email);
        
        var result = await _userService.CreatePlayerAsync(dto);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully created player {PlayerId} with email {Email}", 
                result.Data!.Id, dto.Email);
            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result.Data);
        }
        
        _logger.LogWarning("Failed to create player with email {Email}. Error: {Error}", 
            dto.Email, result.Error);
        return BadRequest(result.Error);
    }

    [HttpPost("agents")]
    [RequireAgent]
    public async Task<ActionResult<AgentDto>> CreateAgent(CreateAgentDto dto)
    {
        _logger.LogInformation("Creating new agent with email: {Email}", dto.Email);
        
        var result = await _userService.CreateAgentAsync(dto);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully created agent {AgentId} with email {Email}", 
                result.Data!.Id, dto.Email);
            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result.Data);
        }
        
        _logger.LogWarning("Failed to create agent with email {Email}. Error: {Error}", 
            dto.Email, result.Error);
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    [RequireUser]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        _logger.LogInformation("Retrieving user with ID: {UserId}", id);
        
        var result = await _userService.GetUserByIdAsync(id);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved user {UserId}", id);
            return Ok(result.Data);
        }
        
        _logger.LogWarning("User {UserId} not found. Error: {Error}", id, result.Error);
        return NotFound(result.Error);
    }
}
