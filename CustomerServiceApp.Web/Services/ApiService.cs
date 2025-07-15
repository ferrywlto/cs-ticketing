using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CustomerServiceApp.Web.Services;

/// <summary>
/// Service for making HTTP calls to the API
/// </summary>
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Sets the authorization header for authenticated requests
    /// </summary>
    /// <param name="token">JWT token</param>
    public void SetAuthorizationHeader(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// Authenticates a player
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>Authentication result</returns>
    public async Task<AuthenticationResultDto?> PlayerLoginAsync(LoginRequestDto loginRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/authentication/player/login", loginRequest, _jsonOptions);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthenticationResultDto>(_jsonOptions);
        }

        return null;
    }

    /// <summary>
    /// Authenticates an agent
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>Authentication result</returns>
    public async Task<AuthenticationResultDto?> AgentLoginAsync(LoginRequestDto loginRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/authentication/agent/login", loginRequest, _jsonOptions);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthenticationResultDto>(_jsonOptions);
        }

        return null;
    }

    /// <summary>
    /// Gets all tickets for the current user
    /// </summary>
    /// <returns>List of tickets</returns>
    public async Task<IReadOnlyList<TicketDto>> GetTicketsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tickets");
            
            if (response.IsSuccessStatusCode)
            {
                var tickets = await response.Content.ReadFromJsonAsync<List<TicketDto>>(_jsonOptions);
                return tickets?.AsReadOnly() ?? new List<TicketDto>().AsReadOnly();
            }

            return [];
        }
        catch
        {
            return new List<TicketDto>().AsReadOnly();
        }
    }

    /// <summary>
    /// Gets a specific ticket by ID
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <returns>Ticket details</returns>
    public async Task<TicketDto?> GetTicketAsync(Guid ticketId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/tickets/{ticketId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TicketDto>(_jsonOptions);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a new ticket
    /// </summary>
    /// <param name="createTicketDto">Ticket creation data</param>
    /// <returns>Created ticket</returns>
    public async Task<TicketDto?> CreateTicketAsync(CreateTicketDto createTicketDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/tickets", createTicketDto, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TicketDto>(_jsonOptions);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Adds a reply to a ticket
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="createReplyDto">Reply data</param>
    /// <returns>Updated ticket with the new reply</returns>
    public async Task<TicketDto?> AddReplyAsync(Guid ticketId, CreateReplyDto createReplyDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/tickets/{ticketId}/replies", createReplyDto, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TicketDto>(_jsonOptions);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves a ticket
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <returns>Updated ticket</returns>
    public async Task<TicketDto?> ResolveTicketAsync(Guid ticketId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"/api/tickets/{ticketId}/resolve", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TicketDto>(_jsonOptions);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
