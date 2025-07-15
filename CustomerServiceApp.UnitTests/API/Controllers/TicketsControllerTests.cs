using CustomerServiceApp.API.Controllers;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Models;
using CustomerServiceApp.Application.Tickets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace CustomerServiceApp.UnitTests.API.Controllers;

public class TicketsControllerTests
{
    private readonly Mock<ITicketService> _mockTicketService;
    private readonly Mock<ILogger<TicketsController>> _mockLogger;
    private readonly TicketsController _controller;

    public TicketsControllerTests()
    {
        _mockTicketService = new Mock<ITicketService>();
        _mockLogger = new Mock<ILogger<TicketsController>>();
        _controller = new TicketsController(_mockTicketService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateTicket_WithValidDto_ReturnsCreatedResult()
    {
        var playerId = Guid.NewGuid();
        var createDto = new CreateTicketDto(
            "Test Ticket",
            "Test Description",
            Guid.Empty); // This will be ignored, playerId from JWT will be used

        // Setup JWT claims for player
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, playerId.ToString()),
            new(ClaimTypes.Role, "Player")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var creatorDto = new PlayerDto(
            playerId,
            "player@example.com",
            "Test Player",
            "P001");

        var ticketDto = new TicketDto(
            Guid.NewGuid(),
            createDto.Title,
            createDto.Description,
            creatorDto,
            "Open",
            DateTime.UtcNow,
            DateTime.UtcNow);

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.CreateTicketAsync(It.Is<CreateTicketDto>(dto => 
            dto.Title == createDto.Title && 
            dto.Description == createDto.Description && 
            dto.CreatorId == playerId)))
                         .ReturnsAsync(result);

        var response = await _controller.CreateTicket(createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var returnedTicket = Assert.IsType<TicketDto>(createdResult.Value);
        Assert.Equal(ticketDto.Id, returnedTicket.Id);
        Assert.Equal(ticketDto.Title, returnedTicket.Title);
    }

    [Fact]
    public async Task CreateTicket_WithFailure_ReturnsBadRequest()
    {
        var playerId = Guid.NewGuid();
        var createDto = new CreateTicketDto(
            "Test Ticket",
            "Test Description",
            Guid.Empty); // This will be ignored

        // Setup JWT claims for player
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, playerId.ToString()),
            new(ClaimTypes.Role, "Player")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var result = Result<TicketDto>.Failure("Creator not found");
        _mockTicketService.Setup(s => s.CreateTicketAsync(It.Is<CreateTicketDto>(dto => 
            dto.Title == createDto.Title && 
            dto.Description == createDto.Description && 
            dto.CreatorId == playerId)))
                         .ReturnsAsync(result);

        var response = await _controller.CreateTicket(createDto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Creator not found", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateTicket_WithInvalidPlayerClaims_ReturnsBadRequest()
    {
        var createDto = new CreateTicketDto(
            "Test Ticket",
            "Test Description",
            Guid.Empty);

        // Setup JWT claims without NameIdentifier (invalid player ID)
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Player")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var response = await _controller.CreateTicket(createDto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Invalid player authentication", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTicket_WithExistingTicket_ReturnsOk()
    {
        var ticketId = Guid.NewGuid();
        var creatorDto = new PlayerDto(
            Guid.NewGuid(),
            "player@example.com",
            "Test Player",
            "P001");

        var ticketDto = new TicketDto(
            ticketId,
            "Test Ticket",
            "Test Description",
            creatorDto,
            "Open",
            DateTime.UtcNow,
            DateTime.UtcNow);

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.GetTicketByIdAsync(ticketId))
                         .ReturnsAsync(result);

        var response = await _controller.GetTicket(ticketId);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTicket = Assert.IsType<TicketDto>(okResult.Value);
        Assert.Equal(ticketId, returnedTicket.Id);
    }

    [Fact]
    public async Task GetTicket_WithNonExistentTicket_ReturnsNotFound()
    {
        var ticketId = Guid.NewGuid();
        var result = Result<TicketDto>.Failure("Ticket not found");
        _mockTicketService.Setup(s => s.GetTicketByIdAsync(ticketId))
                         .ReturnsAsync(result);

        var response = await _controller.GetTicket(ticketId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("Ticket not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetTicketsByPlayer_WithExistingPlayer_ReturnsOk()
    {
        var playerId = Guid.NewGuid();
        var tickets = new List<TicketSummaryDto>
        {
            new TicketSummaryDto(
                Guid.NewGuid(),
                "Ticket 1",
                "Description 1",
                new PlayerDto(
                    playerId,
                    "player@example.com",
                    "Test Player",
                    "P001"),
                "Open",
                DateTime.UtcNow,
                DateTime.UtcNow,
                1)
        };

        var result = Result<IEnumerable<TicketSummaryDto>>.Success(tickets);
        _mockTicketService.Setup(s => s.GetPlayerTicketsAsync(playerId))
                         .ReturnsAsync(result);

        var response = await _controller.GetTicketsByPlayer(playerId);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTickets = Assert.IsType<List<TicketSummaryDto>>(okResult.Value);
        Assert.Single(returnedTickets);
    }

    [Fact]
    public async Task GetUnresolvedTickets_ForAgents_ReturnsOk()
    {
        var tickets = new List<TicketSummaryDto>
        {
            new TicketSummaryDto(
                Guid.NewGuid(),
                "Open Ticket",
                "Open ticket description",
                new PlayerDto(
                    Guid.NewGuid(),
                    "player1@example.com",
                    "Player 1",
                    "P001"),
                "Open",
                DateTime.UtcNow,
                DateTime.UtcNow,
                1),
            new TicketSummaryDto(
                Guid.NewGuid(),
                "In Resolution Ticket",
                "In resolution ticket description",
                new PlayerDto(
                    Guid.NewGuid(),
                    "player2@example.com",
                    "Player 2",
                    "P002"),
                "InResolution",
                DateTime.UtcNow,
                DateTime.UtcNow,
                3)
        };

        var result = Result<IEnumerable<TicketSummaryDto>>.Success(tickets);
        _mockTicketService.Setup(s => s.GetUnresolvedTicketsAsync())
                         .ReturnsAsync(result);

        var response = await _controller.GetUnresolvedTickets();

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTickets = Assert.IsType<List<TicketSummaryDto>>(okResult.Value);
        Assert.Equal(2, returnedTickets.Count);
        Assert.Contains(returnedTickets, t => t.Status == "Open");
        Assert.Contains(returnedTickets, t => t.Status == "InResolution");
    }

    [Fact]
    public async Task ResolveTicket_WithValidTicket_ReturnsOk()
    {
        var ticketId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Setup JWT claims for agent
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, agentId.ToString()),
            new(ClaimTypes.Role, "Agent")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var resolvedByAgent = new AgentDto(
            agentId,
            "agent@example.com",
            "Test Agent");

        var ticketDto = new TicketDto(
            ticketId,
            "Test Ticket",
            "Test Description",
            new PlayerDto(
                Guid.NewGuid(),
                "player@example.com",
                "Test Player",
                "P001"),
            "Resolved",
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            resolvedByAgent);

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.ResolveTicketAsync(ticketId, agentId))
                         .ReturnsAsync(result);

        var response = await _controller.ResolveTicket(ticketId);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTicket = Assert.IsType<TicketDto>(okResult.Value);
        Assert.Equal(ticketId, returnedTicket.Id);
        Assert.Equal("Resolved", returnedTicket.Status);
        Assert.Equal(agentId, returnedTicket.ResolvedBy?.Id);
    }

    [Fact]
    public async Task ResolveTicket_WithTicketNotInResolution_ReturnsBadRequest()
    {
        var ticketId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Setup JWT claims for agent
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, agentId.ToString()),
            new(ClaimTypes.Role, "Agent")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var result = Result<TicketDto>.Failure("Can only resolve tickets that are in resolution status");
        _mockTicketService.Setup(s => s.ResolveTicketAsync(ticketId, agentId))
                         .ReturnsAsync(result);

        var response = await _controller.ResolveTicket(ticketId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Can only resolve tickets that are in resolution status", badRequestResult.Value);
    }

    [Fact]
    public async Task AddReply_WithValidData_ReturnsOk()
    {
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createReplyDto = new CreateReplyDto(
            "This is a reply",
            Guid.Empty, // This will be ignored
            ticketId);

        // Setup JWT claims for user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, "Agent")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var replyDto = new ReplyDto(
            Guid.NewGuid(),
            createReplyDto.Content,
            new AgentDto(
                userId,
                "agent@example.com",
                "CS Agent"),
            DateTime.UtcNow);

        var result = Result<ReplyDto>.Success(replyDto);
        _mockTicketService.Setup(s => s.AddReplyAsync(ticketId, It.Is<CreateReplyDto>(dto => 
            dto.Content == createReplyDto.Content && 
            dto.AuthorId == userId &&
            dto.TicketId == createReplyDto.TicketId)))
                         .ReturnsAsync(result);

        var response = await _controller.AddReply(ticketId, createReplyDto);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedReply = Assert.IsType<ReplyDto>(okResult.Value);
        Assert.Equal(replyDto.Id, returnedReply.Id);
        Assert.Equal(replyDto.Content, returnedReply.Content);
    }

    [Fact]
    public async Task AddReply_WithInvalidTicket_ReturnsNotFound()
    {
        var ticketId = Guid.NewGuid();
        var createReplyDto = new CreateReplyDto(
            "This is a reply",
            Guid.NewGuid(),
            ticketId);

        var result = Result<ReplyDto>.Failure("Ticket not found");
        _mockTicketService.Setup(s => s.AddReplyAsync(ticketId, createReplyDto))
                         .ReturnsAsync(result);

        var response = await _controller.AddReply(ticketId, createReplyDto);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("Ticket not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateTicket_LogsInformation()
    {
        var createDto = new CreateTicketDto(
            "Test Ticket",
            "Test Description",
            Guid.NewGuid());

        var creatorDto = new PlayerDto(
            createDto.CreatorId,
            "player@example.com",
            "Test Player",
            "P001");

        var ticketDto = new TicketDto(
            Guid.NewGuid(),
            createDto.Title,
            createDto.Description,
            creatorDto,
            "Open",
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.CreateTicketAsync(createDto))
                         .ReturnsAsync(result);

        await _controller.CreateTicket(createDto);

        // Verify information logging for ticket creation attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating new ticket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful creation
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully created ticket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTicket_WithFailure_LogsWarning()
    {
        var createDto = new CreateTicketDto(
            "Test Ticket",
            "Test Description",
            Guid.NewGuid());

        var result = Result<TicketDto>.Failure("Creator not found");
        _mockTicketService.Setup(s => s.CreateTicketAsync(createDto))
                         .ReturnsAsync(result);

        await _controller.CreateTicket(createDto);

        // Verify warning logging for failed creation
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create ticket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTicket_LogsInformation()
    {
        var ticketId = Guid.NewGuid();
        var creatorDto = new PlayerDto(
            Guid.NewGuid(),
            "player@example.com",
            "Test Player",
            "P001");

        var ticketDto = new TicketDto(
            ticketId,
            "Test Ticket",
            "Test Description",
            creatorDto,
            "Open",
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.GetTicketByIdAsync(ticketId))
                         .ReturnsAsync(result);

        await _controller.GetTicket(ticketId);

        // Verify information logging for retrieval attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving ticket with ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful retrieval
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved ticket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTicket_WithNonExistentTicket_LogsWarning()
    {
        var ticketId = Guid.NewGuid();
        var result = Result<TicketDto>.Failure("Ticket not found");
        _mockTicketService.Setup(s => s.GetTicketByIdAsync(ticketId))
                         .ReturnsAsync(result);

        await _controller.GetTicket(ticketId);

        // Verify warning logging for ticket not found
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ticket") && v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTicketsByPlayer_LogsInformation()
    {
        var playerId = Guid.NewGuid();
        var tickets = new List<TicketSummaryDto>
        {
            new TicketSummaryDto(
                Guid.NewGuid(),
                "Ticket 1",
                "Description 1",
                new PlayerDto(
                    playerId,
                    "player@example.com",
                    "Test Player",
                    "P001"),
                "Open",
                DateTime.UtcNow,
                DateTime.UtcNow,
                1)
        };

        var result = Result<IEnumerable<TicketSummaryDto>>.Success(tickets);
        _mockTicketService.Setup(s => s.GetPlayerTicketsAsync(playerId))
                         .ReturnsAsync(result);

        await _controller.GetTicketsByPlayer(playerId);

        // Verify information logging for retrieval attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving tickets for player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful retrieval with count
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved") && v.ToString()!.Contains("tickets for player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUnresolvedTickets_LogsInformation()
    {
        var tickets = new List<TicketSummaryDto>
        {
            new TicketSummaryDto(
                Guid.NewGuid(),
                "Open Ticket",
                "Open ticket description",
                new PlayerDto(
                    Guid.NewGuid(),
                    "player1@example.com",
                    "Player 1",
                    "P001"),
                "Open",
                DateTime.UtcNow,
                DateTime.UtcNow,
                1)
        };

        var result = Result<IEnumerable<TicketSummaryDto>>.Success(tickets);
        _mockTicketService.Setup(s => s.GetUnresolvedTicketsAsync())
                         .ReturnsAsync(result);

        await _controller.GetUnresolvedTickets();

        // Verify information logging for retrieval attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrieving all unresolved tickets")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful retrieval with count
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully retrieved") && v.ToString()!.Contains("unresolved tickets")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddReply_LogsInformation()
    {
        var ticketId = Guid.NewGuid();
        var createReplyDto = new CreateReplyDto(
            "This is a reply",
            Guid.NewGuid(),
            ticketId);

        var replyDto = new ReplyDto(
            Guid.NewGuid(),
            createReplyDto.Content,
            new AgentDto(
                createReplyDto.AuthorId,
                "agent@example.com",
                "CS Agent"),
            DateTime.UtcNow);

        var result = Result<ReplyDto>.Success(replyDto);
        _mockTicketService.Setup(s => s.AddReplyAsync(ticketId, createReplyDto))
                         .ReturnsAsync(result);

        await _controller.AddReply(ticketId, createReplyDto);

        // Verify information logging for reply addition attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Adding reply to ticket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful reply addition
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully added reply")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddReply_WithInvalidTicket_LogsWarning()
    {
        var ticketId = Guid.NewGuid();
        var createReplyDto = new CreateReplyDto(
            "This is a reply",
            Guid.NewGuid(),
            ticketId);

        var result = Result<ReplyDto>.Failure("Ticket not found");
        _mockTicketService.Setup(s => s.AddReplyAsync(ticketId, createReplyDto))
                         .ReturnsAsync(result);

        await _controller.AddReply(ticketId, createReplyDto);

        // Verify warning logging for ticket not found
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found when adding reply")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolveTicket_LogsInformation()
    {
        var ticketId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Setup JWT claims for agent
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, agentId.ToString()),
            new(ClaimTypes.Role, "Agent")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var resolvedByAgent = new AgentDto(
            agentId,
            "agent@example.com",
            "Test Agent");

        var ticketDto = new TicketDto(
            ticketId,
            "Test Ticket",
            "Test Description",
            new PlayerDto(
                Guid.NewGuid(),
                "player@example.com",
                "Test Player",
                "P001"),
            "Resolved",
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            resolvedByAgent);

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.ResolveTicketAsync(ticketId, agentId))
                         .ReturnsAsync(result);

        await _controller.ResolveTicket(ticketId);

        // Verify information logging for resolution attempt
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Resolving ticket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify information logging for successful resolution
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully resolved ticket") && v.ToString()!.Contains("by agent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolveTicket_WithTicketNotInResolution_LogsWarning()
    {
        var ticketId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Setup JWT claims for agent
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, agentId.ToString()),
            new(ClaimTypes.Role, "Agent")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var result = Result<TicketDto>.Failure("Can only resolve tickets that are in resolution status");
        _mockTicketService.Setup(s => s.ResolveTicketAsync(ticketId, agentId))
                         .ReturnsAsync(result);

        await _controller.ResolveTicket(ticketId);

        // Verify warning logging for failed resolution
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to resolve ticket") && v.ToString()!.Contains("by agent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolveTicket_WithInvalidAgentClaims_ReturnsBadRequest()
    {
        var ticketId = Guid.NewGuid();

        // Setup JWT claims without NameIdentifier (invalid agent ID)
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Agent")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var response = await _controller.ResolveTicket(ticketId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Invalid agent authentication", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTicketsByPlayer_WithFailure_LogsWarning()
    {
        var playerId = Guid.NewGuid();
        var result = Result<IEnumerable<TicketSummaryDto>>.Failure("Player not found");
        _mockTicketService.Setup(s => s.GetPlayerTicketsAsync(playerId))
                         .ReturnsAsync(result);

        var response = await _controller.GetTicketsByPlayer(playerId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Player not found", badRequestResult.Value);

        // Verify warning logging for failed retrieval
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to retrieve tickets for player")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUnresolvedTickets_WithFailure_LogsWarning()
    {
        var result = Result<IEnumerable<TicketSummaryDto>>.Failure("Database error");
        _mockTicketService.Setup(s => s.GetUnresolvedTicketsAsync())
                         .ReturnsAsync(result);

        var response = await _controller.GetUnresolvedTickets();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Database error", badRequestResult.Value);

        // Verify warning logging for failed retrieval
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to retrieve unresolved tickets")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddReply_WithGeneralFailure_LogsWarning()
    {
        var ticketId = Guid.NewGuid();
        var createReplyDto = new CreateReplyDto(
            "This is a reply",
            Guid.NewGuid(),
            ticketId);

        var result = Result<ReplyDto>.Failure("Validation error");
        _mockTicketService.Setup(s => s.AddReplyAsync(ticketId, createReplyDto))
                         .ReturnsAsync(result);

        var response = await _controller.AddReply(ticketId, createReplyDto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Validation error", badRequestResult.Value);

        // Verify warning logging for failed reply addition
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to add reply to ticket")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
