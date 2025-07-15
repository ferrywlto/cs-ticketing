using CustomerServiceApp.Web.State;
using CustomerServiceApp.Web.Services;
using CustomerServiceApp.Application.Authentication;
using CustomerServiceApp.Application.Common.DTOs;
using Microsoft.JSInterop;
using Xunit;
using Moq;
using System.Text.Json;

namespace CustomerServiceApp.UnitTests.Web.State;

public class AppStateStoreWithLocalStorageTests
{
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly Mock<ILocalStorageService> _mockLocalStorageService;
    private readonly AppStateStore _appStateStore;

    public AppStateStoreWithLocalStorageTests()
    {
        _mockJSRuntime = new Mock<IJSRuntime>();
        _mockLocalStorageService = new Mock<ILocalStorageService>();
        _appStateStore = new AppStateStore(_mockLocalStorageService.Object);
    }

    [Fact]
    public void DispatchLogin_ShouldPersistStateToLocalStorage()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var playerDto = new PlayerDto(
            playerId,
            "test@example.com",
            "Test Player",
            "PLAYER001"
        );
        
        var authResult = new AuthenticationResultDto(
            playerDto,
            "test-token",
            DateTime.UtcNow.AddHours(1)
        );

        // Act
        _appStateStore.DispatchLogin(authResult);

        // Assert
        _mockLocalStorageService.Verify(
            x => x.SetItemAsync("app-state", It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void DispatchLogout_ShouldClearStateFromLocalStorage()
    {
        // Act
        _appStateStore.DispatchLogout();

        // Assert
        _mockLocalStorageService.Verify(
            x => x.RemoveItemAsync("app-state"),
            Times.Once);
    }

    [Fact]
    public async Task LoadStateFromLocalStorage_ShouldRestorePersistedState()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var playerDto = new PlayerDto(
            playerId,
            "test@example.com",
            "Test Player",
            "PLAYER001"
        );
        
        var persistedState = new AppState
        {
            CurrentUser = playerDto,
            Token = "test-token",
            TokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var serializedState = JsonSerializer.Serialize(persistedState);
        _mockLocalStorageService.Setup(x => x.GetItemAsync("app-state"))
            .ReturnsAsync(serializedState);

        // Act
        await _appStateStore.LoadStateFromLocalStorageAsync();
        var state = _appStateStore.GetState();

        // Assert
        Assert.NotNull(state.CurrentUser);
        Assert.Equal("test@example.com", state.CurrentUser.Email);
        Assert.Equal("test-token", state.Token);
        Assert.True(state.IsAuthenticated);
    }

    [Fact]
    public async Task LoadStateFromLocalStorage_WithExpiredToken_ShouldClearState()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var playerDto = new PlayerDto(
            playerId,
            "test@example.com",
            "Test Player",
            "PLAYER001"
        );
        
        var expiredState = new AppState
        {
            CurrentUser = playerDto,
            Token = "expired-token",
            TokenExpiresAt = DateTime.UtcNow.AddHours(-1) // Expired
        };

        var serializedState = JsonSerializer.Serialize(expiredState);
        _mockLocalStorageService.Setup(x => x.GetItemAsync("app-state"))
            .ReturnsAsync(serializedState);

        // Act
        await _appStateStore.LoadStateFromLocalStorageAsync();
        var state = _appStateStore.GetState();

        // Assert
        Assert.Null(state.CurrentUser);
        Assert.Null(state.Token);
        Assert.False(state.IsAuthenticated);
        _mockLocalStorageService.Verify(
            x => x.RemoveItemAsync("app-state"),
            Times.Once);
    }

    [Fact]
    public async Task LoadStateFromLocalStorage_WithInvalidData_ShouldHandleGracefully()
    {
        // Arrange
        _mockLocalStorageService.Setup(x => x.GetItemAsync("app-state"))
            .ReturnsAsync("invalid-json");

        // Act
        await _appStateStore.LoadStateFromLocalStorageAsync();
        var state = _appStateStore.GetState();

        // Assert
        Assert.Null(state.CurrentUser);
        Assert.Null(state.Token);
        Assert.False(state.IsAuthenticated);
    }

    [Fact]
    public async Task LoadStateFromLocalStorage_WithNullData_ShouldHandleGracefully()
    {
        // Arrange
        _mockLocalStorageService.Setup(x => x.GetItemAsync("app-state"))
            .ReturnsAsync((string?)null);

        // Act
        await _appStateStore.LoadStateFromLocalStorageAsync();
        var state = _appStateStore.GetState();

        // Assert
        Assert.Null(state.CurrentUser);
        Assert.Null(state.Token);
        Assert.False(state.IsAuthenticated);
    }
}
