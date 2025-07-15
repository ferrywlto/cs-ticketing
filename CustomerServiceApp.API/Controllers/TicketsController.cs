using CustomerServiceApp.API.Authorization;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Tickets;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CustomerServiceApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    [HttpPost]
    [RequirePlayer]
    public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto dto)
    {
        _logger.LogInformation("Creating new ticket with title: {Title} for player: {CreatorId}", 
            dto.Title, dto.CreatorId);
            
        var result = await _ticketService.CreateTicketAsync(dto);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully created ticket {TicketId} for player {CreatorId}", 
                result.Data!.Id, dto.CreatorId);
            return CreatedAtAction(nameof(GetTicket), new { id = result.Data!.Id }, result.Data);
        }
        
        _logger.LogWarning("Failed to create ticket for player {CreatorId}. Error: {Error}", 
            dto.CreatorId, result.Error);
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    [RequireUser]
    public async Task<ActionResult<TicketDto>> GetTicket(Guid id)
    {
        _logger.LogInformation("Retrieving ticket with ID: {TicketId}", id);
        
        var result = await _ticketService.GetTicketByIdAsync(id);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved ticket {TicketId}", id);
            return Ok(result.Data);
        }
        
        _logger.LogWarning("Ticket {TicketId} not found. Error: {Error}", id, result.Error);
        return NotFound(result.Error);
    }

    [HttpGet("player/{playerId}")]
    [RequireUser]
    public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetTicketsByPlayer(Guid playerId)
    {
        _logger.LogInformation("Retrieving tickets for player: {PlayerId}", playerId);
        
        var result = await _ticketService.GetPlayerTicketsAsync(playerId);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved {TicketCount} tickets for player {PlayerId}", 
                result.Data?.Count() ?? 0, playerId);
            return Ok(result.Data);
        }
        
        _logger.LogWarning("Failed to retrieve tickets for player {PlayerId}. Error: {Error}", 
            playerId, result.Error);
        return BadRequest(result.Error);
    }

    [HttpGet("unresolved")]
    [RequireAgent]
    public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetUnresolvedTickets()
    {
        _logger.LogInformation("Retrieving all unresolved tickets");
        
        var result = await _ticketService.GetUnresolvedTicketsAsync();
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved {TicketCount} unresolved tickets", 
                result.Data?.Count() ?? 0);
            return Ok(result.Data);
        }
        
        _logger.LogWarning("Failed to retrieve unresolved tickets. Error: {Error}", result.Error);
        return BadRequest(result.Error);
    }

    [HttpPost("{ticketId}/replies")]
    [RequireUser]
    public async Task<ActionResult<ReplyDto>> AddReply(Guid ticketId, CreateReplyDto dto)
    {
        _logger.LogInformation("Adding reply to ticket {TicketId} by user {UserId}", 
            ticketId, dto.AuthorId);
            
        var result = await _ticketService.AddReplyAsync(ticketId, dto);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully added reply {ReplyId} to ticket {TicketId}", 
                result.Data!.Id, ticketId);
            return Ok(result.Data);
        }
        
        if (result.Error?.Contains("not found") == true)
        {
            _logger.LogWarning("Ticket {TicketId} not found when adding reply", ticketId);
            return NotFound(result.Error);
        }
        
        _logger.LogWarning("Failed to add reply to ticket {TicketId}. Error: {Error}", 
            ticketId, result.Error);
        return BadRequest(result.Error);
    }

    [HttpPut("{id}/resolve")]
    [RequireAgent]
    public async Task<ActionResult<TicketDto>> ResolveTicket(Guid id)
    {
        _logger.LogInformation("Resolving ticket {TicketId}", id);
        
        // Extract agent ID from JWT token claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var agentId))
        {
            _logger.LogWarning("Failed to extract agent ID from token for ticket resolution");
            return BadRequest("Invalid agent authentication");
        }
        
        var result = await _ticketService.ResolveTicketAsync(id, agentId);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully resolved ticket {TicketId} by agent {AgentId}", id, agentId);
            return Ok(result.Data);
        }
        
        _logger.LogWarning("Failed to resolve ticket {TicketId} by agent {AgentId}. Error: {Error}", 
            id, agentId, result.Error);
        return BadRequest(result.Error);
    }
}
