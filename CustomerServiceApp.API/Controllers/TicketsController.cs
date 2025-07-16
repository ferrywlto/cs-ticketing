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

    private (Guid userId, string role, ActionResult? errorResult) ExtractUserClaims(string operationName)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var userRoleClaim = User.FindFirst(ClaimTypes.Role);
        
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Failed to extract user ID from token for {OperationName}", operationName);
            return (Guid.Empty, string.Empty, BadRequest("Invalid user authentication"));
        }
        
        if (userRoleClaim == null)
        {
            _logger.LogWarning("Failed to extract user role from token for {OperationName}", operationName);
            return (Guid.Empty, string.Empty, BadRequest("Invalid user authentication"));
        }
        
        return (userId, userRoleClaim.Value, null);
    }

    [HttpPost]
    [RequirePlayer]
    public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract player ID from JWT token claims
        var (playerId, _, errorResult) = ExtractUserClaims("CreateTicket");
        if (errorResult != null)
        {
            return errorResult;
        }

        // Create a new DTO with the authenticated player ID
        var authenticatedDto = new CreateTicketDto(dto.Title, dto.Description, playerId);
        
        _logger.LogInformation("Creating new ticket with title: {Title} for player: {CreatorId}", 
            authenticatedDto.Title, authenticatedDto.CreatorId);
            
        var result = await _ticketService.CreateTicketAsync(authenticatedDto);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully created ticket {TicketId} for player {CreatorId}", 
                result.Data!.Id, authenticatedDto.CreatorId);
            return CreatedAtAction(nameof(GetTicket), new { id = result.Data!.Id }, result.Data);
        }
        
        _logger.LogWarning("Failed to create ticket for player {CreatorId}. Error: {Error}", 
            authenticatedDto.CreatorId, result.Error);
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
            // Extract user ID and role from JWT token claims for additional security check
            var (userId, role, errorResult) = ExtractUserClaims("GetTicket");
            if (errorResult != null)
            {
                return errorResult;
            }
            
            // If user is a player, they can only access their own tickets
            if (role == "Player" && result.Data!.Creator.Id != userId)
            {
                _logger.LogWarning("Player {UserId} attempted to access ticket {TicketId} owned by player {OwnerId}", 
                    userId, id, result.Data!.Creator.Id);
                return Unauthorized("Players can only access their own tickets");
            }
            
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
        
        // Extract user ID and role from JWT token claims
        var (userId, role, errorResult) = ExtractUserClaims("GetTicketsByPlayer");
        if (errorResult != null)
        {
            return errorResult;
        }
        
        // If user is a player, they can only access their own tickets
        if (role == "Player" && userId != playerId)
        {
            _logger.LogWarning("Player {UserId} attempted to access tickets for player {PlayerId}", 
                userId, playerId);
            return Unauthorized("Players can only access their own tickets");
        }
        
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract user ID and role from JWT token claims
        var (userId, role, errorResult) = ExtractUserClaims("AddReply");
        if (errorResult != null)
        {
            return errorResult;
        }

        // If user is a player, check if they can reply to this ticket
        if (role == "Player")
        {
            // First, get the ticket to check ownership
            var ticketResult = await _ticketService.GetTicketByIdAsync(ticketId);
            if (!ticketResult.IsSuccess)
            {
                if (ticketResult.Error?.Contains("not found") == true)
                {
                    _logger.LogWarning("Ticket {TicketId} not found when adding reply", ticketId);
                    return NotFound(ticketResult.Error);
                }
                _logger.LogWarning("Failed to retrieve ticket {TicketId} for reply validation. Error: {Error}", 
                    ticketId, ticketResult.Error);
                return BadRequest(ticketResult.Error);
            }

            // Players can only reply to their own tickets
            if (ticketResult.Data!.Creator.Id != userId)
            {
                _logger.LogWarning("Player {UserId} attempted to reply to ticket {TicketId} owned by player {OwnerId}", 
                    userId, ticketId, ticketResult.Data!.Creator.Id);
                return Unauthorized("Players can only reply to their own tickets");
            }
        }

        // Create a new DTO with the authenticated user ID
        var authenticatedDto = new CreateReplyDto(dto.Content, userId, dto.TicketId);
        
        _logger.LogInformation("Adding reply to ticket {TicketId} by user {UserId}", 
            ticketId, authenticatedDto.AuthorId);
            
        var result = await _ticketService.AddReplyAsync(ticketId, authenticatedDto);
        
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
        var (agentId, _, errorResult) = ExtractUserClaims("ResolveTicket");
        if (errorResult != null)
        {
            return errorResult;
        }
        
        var result = await _ticketService.ResolveTicketAsync(id, agentId);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully resolved ticket {TicketId} by agent {AgentId}", id, agentId);
            return Ok(result.Data);
        }
        
        if (result.Error?.Contains("not found") == true)
        {
            _logger.LogWarning("Ticket {TicketId} not found for resolution by agent {AgentId}", id, agentId);
            return NotFound(result.Error);
        }
        
        _logger.LogWarning("Failed to resolve ticket {TicketId} by agent {AgentId}. Error: {Error}", 
            id, agentId, result.Error);
        return BadRequest(result.Error);
    }
}
