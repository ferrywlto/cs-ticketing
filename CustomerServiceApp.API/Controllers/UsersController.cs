using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
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
    public async Task<ActionResult<PlayerDto>> CreatePlayer(CreatePlayerDto dto)
    {
        var result = await _userService.CreatePlayerAsync(dto);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result.Data);
        }
        
        return BadRequest(result.Error);
    }

    [HttpPost("agents")]
    public async Task<ActionResult<AgentDto>> CreateAgent(CreateAgentDto dto)
    {
        var result = await _userService.CreateAgentAsync(dto);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result.Data);
        }
        
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return NotFound(result.Error);
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<UserDto>> Authenticate(string email, string password)
    {
        var result = await _userService.AuthenticateAsync(email, password);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return Unauthorized(result.Error);
    }
}
