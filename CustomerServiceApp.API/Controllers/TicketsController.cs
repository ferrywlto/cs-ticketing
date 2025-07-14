using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto dto)
    {
        var result = await _ticketService.CreateTicketAsync(dto);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetTicket), new { id = result.Data!.Id }, result.Data);
        }
        
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDto>> GetTicket(Guid id)
    {
        var result = await _ticketService.GetTicketByIdAsync(id);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return NotFound(result.Error);
    }

    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetTicketsByPlayer(Guid playerId)
    {
        var result = await _ticketService.GetPlayerTicketsAsync(playerId);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return BadRequest(result.Error);
    }

    [HttpGet("unresolved")]
    public async Task<ActionResult<IEnumerable<TicketSummaryDto>>> GetUnresolvedTickets()
    {
        var result = await _ticketService.GetUnresolvedTicketsAsync();
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return BadRequest(result.Error);
    }

    [HttpPost("{ticketId}/replies")]
    public async Task<ActionResult<ReplyDto>> AddReply(Guid ticketId, CreateReplyDto dto)
    {
        var result = await _ticketService.AddReplyAsync(ticketId, dto);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        if (result.Error?.Contains("not found") == true)
        {
            return NotFound(result.Error);
        }
        
        return BadRequest(result.Error);
    }

    [HttpPut("{id}/resolve")]
    public async Task<ActionResult<TicketDto>> ResolveTicket(Guid id)
    {
        var result = await _ticketService.ResolveTicketAsync(id);
        
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        
        return BadRequest(result.Error);
    }
}
