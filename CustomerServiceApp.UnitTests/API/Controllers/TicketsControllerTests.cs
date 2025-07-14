using CustomerServiceApp.API.Controllers;
using CustomerServiceApp.Application.Common.DTOs;
using CustomerServiceApp.Application.Common.Interfaces;
using CustomerServiceApp.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
        // Arrange
        var createDto = new CreateTicketDto
        {
            Title = "Test Ticket",
            Description = "Test Description",
            CreatorId = Guid.NewGuid()
        };

        var creatorDto = new PlayerDto
        {
            Id = createDto.CreatorId,
            Email = "player@example.com",
            Name = "Test Player",
            PlayerNumber = "P001"
        };

        var ticketDto = new TicketDto
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Description = createDto.Description,
            Creator = creatorDto,
            CreatedDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow
        };

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.CreateTicketAsync(createDto))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateTicket(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var returnedTicket = Assert.IsType<TicketDto>(createdResult.Value);
        Assert.Equal(ticketDto.Id, returnedTicket.Id);
        Assert.Equal(ticketDto.Title, returnedTicket.Title);
    }

    [Fact]
    public async Task CreateTicket_WithFailure_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateTicketDto
        {
            Title = "Test Ticket",
            Description = "Test Description",
            CreatorId = Guid.NewGuid()
        };

        var result = Result<TicketDto>.Failure("Creator not found");
        _mockTicketService.Setup(s => s.CreateTicketAsync(createDto))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateTicket(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Creator not found", badRequestResult.Value);
    }

    [Fact]
    public async Task GetTicket_WithExistingTicket_ReturnsOk()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var creatorDto = new PlayerDto
        {
            Id = Guid.NewGuid(),
            Email = "player@example.com",
            Name = "Test Player",
            PlayerNumber = "P001"
        };

        var ticketDto = new TicketDto
        {
            Id = ticketId,
            Title = "Test Ticket",
            Description = "Test Description",
            Creator = creatorDto,
            CreatedDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow
        };

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.GetTicketByIdAsync(ticketId))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.GetTicket(ticketId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTicket = Assert.IsType<TicketDto>(okResult.Value);
        Assert.Equal(ticketId, returnedTicket.Id);
    }

    [Fact]
    public async Task GetTicket_WithNonExistentTicket_ReturnsNotFound()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var result = Result<TicketDto>.Failure("Ticket not found");
        _mockTicketService.Setup(s => s.GetTicketByIdAsync(ticketId))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.GetTicket(ticketId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("Ticket not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetTicketsByPlayer_WithExistingPlayer_ReturnsOk()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var tickets = new List<TicketSummaryDto>
        {
            new TicketSummaryDto
            {
                Id = Guid.NewGuid(),
                Title = "Ticket 1",
                Description = "Description 1",
                Creator = new PlayerDto
                {
                    Id = playerId,
                    Email = "player@example.com",
                    Name = "Test Player",
                    PlayerNumber = "P001"
                },
                CreatedDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                MessageCount = 1
            }
        };

        var result = Result<IEnumerable<TicketSummaryDto>>.Success(tickets);
        _mockTicketService.Setup(s => s.GetPlayerTicketsAsync(playerId))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.GetTicketsByPlayer(playerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTickets = Assert.IsType<List<TicketSummaryDto>>(okResult.Value);
        Assert.Single(returnedTickets);
    }

    [Fact]
    public async Task GetUnresolvedTickets_ForAgents_ReturnsOk()
    {
        // Arrange
        var tickets = new List<TicketSummaryDto>
        {
            new TicketSummaryDto
            {
                Id = Guid.NewGuid(),
                Title = "Open Ticket",
                Description = "Open ticket description",
                Creator = new PlayerDto
                {
                    Id = Guid.NewGuid(),
                    Email = "player1@example.com",
                    Name = "Player 1",
                    PlayerNumber = "P001"
                },
                Status = "Open",
                CreatedDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                MessageCount = 1
            },
            new TicketSummaryDto
            {
                Id = Guid.NewGuid(),
                Title = "In Resolution Ticket",
                Description = "In resolution ticket description",
                Creator = new PlayerDto
                {
                    Id = Guid.NewGuid(),
                    Email = "player2@example.com",
                    Name = "Player 2",
                    PlayerNumber = "P002"
                },
                Status = "InResolution",
                CreatedDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                MessageCount = 3
            }
        };

        var result = Result<IEnumerable<TicketSummaryDto>>.Success(tickets);
        _mockTicketService.Setup(s => s.GetUnresolvedTicketsAsync())
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.GetUnresolvedTickets();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTickets = Assert.IsType<List<TicketSummaryDto>>(okResult.Value);
        Assert.Equal(2, returnedTickets.Count);
        Assert.Contains(returnedTickets, t => t.Status == "Open");
        Assert.Contains(returnedTickets, t => t.Status == "InResolution");
    }

    [Fact]
    public async Task ResolveTicket_WithValidTicket_ReturnsOk()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        var ticketDto = new TicketDto
        {
            Id = ticketId,
            Title = "Test Ticket",
            Description = "Test Description",
            Creator = new PlayerDto
            {
                Id = Guid.NewGuid(),
                Email = "player@example.com",
                Name = "Test Player",
                PlayerNumber = "P001"
            },
            Status = "Resolved",
            CreatedDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow,
            ResolvedDate = DateTime.UtcNow
        };

        var result = Result<TicketDto>.Success(ticketDto);
        _mockTicketService.Setup(s => s.ResolveTicketAsync(ticketId))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.ResolveTicket(ticketId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedTicket = Assert.IsType<TicketDto>(okResult.Value);
        Assert.Equal(ticketId, returnedTicket.Id);
        Assert.Equal("Resolved", returnedTicket.Status);
    }

    [Fact]
    public async Task ResolveTicket_WithTicketNotInResolution_ReturnsBadRequest()
    {
        // Arrange
        var ticketId = Guid.NewGuid();

        var result = Result<TicketDto>.Failure("Can only resolve tickets that are in resolution status");
        _mockTicketService.Setup(s => s.ResolveTicketAsync(ticketId))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.ResolveTicket(ticketId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("Can only resolve tickets that are in resolution status", badRequestResult.Value);
    }

    [Fact]
    public async Task AddReply_WithValidData_ReturnsOk()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var createReplyDto = new CreateReplyDto
        {
            Content = "This is a reply",
            AuthorId = Guid.NewGuid(),
            TicketId = ticketId
        };

        var replyDto = new ReplyDto
        {
            Id = Guid.NewGuid(),
            Content = createReplyDto.Content,
            Author = new AgentDto
            {
                Id = createReplyDto.AuthorId,
                Email = "agent@example.com",
                Name = "CS Agent"
            },
            CreatedDate = DateTime.UtcNow
        };

        var result = Result<ReplyDto>.Success(replyDto);
        _mockTicketService.Setup(s => s.AddReplyAsync(ticketId, createReplyDto))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.AddReply(ticketId, createReplyDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var returnedReply = Assert.IsType<ReplyDto>(okResult.Value);
        Assert.Equal(replyDto.Id, returnedReply.Id);
        Assert.Equal(replyDto.Content, returnedReply.Content);
    }

    [Fact]
    public async Task AddReply_WithInvalidTicket_ReturnsNotFound()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var createReplyDto = new CreateReplyDto
        {
            Content = "This is a reply",
            AuthorId = Guid.NewGuid(),
            TicketId = ticketId
        };

        var result = Result<ReplyDto>.Failure("Ticket not found");
        _mockTicketService.Setup(s => s.AddReplyAsync(ticketId, createReplyDto))
                         .ReturnsAsync(result);

        // Act
        var response = await _controller.AddReply(ticketId, createReplyDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Equal("Ticket not found", notFoundResult.Value);
    }
}
