using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using CustomerServiceApp.Web.Services;
using CustomerServiceApp.UnitTests.Common;

namespace CustomerServiceApp.UnitTests.Web.Services;

public class LocalStorageServiceTests
{
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly Mock<ILogger<LocalStorageService>> _mockLogger;
    private readonly LocalStorageService _localStorageService;

    public LocalStorageServiceTests()
    {
        _mockJSRuntime = new Mock<IJSRuntime>();
        _mockLogger = new Mock<ILogger<LocalStorageService>>();
        _localStorageService = new LocalStorageService(_mockJSRuntime.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        const string key = "testKey";
        const string expectedValue = "testValue";
        _mockJSRuntime.Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _localStorageService.GetItemAsync(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetItemAsync_ShouldLogException_WhenJSExceptionThrown()
    {
        // Arrange
        const string key = "testKey";
        var jsException = new JSException("JS Error");
        _mockJSRuntime.Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ThrowsAsync(jsException);

        // Act
        var result = await _localStorageService.GetItemAsync(key);

        // Assert
        Assert.Null(result);
        _mockLogger.VerifyLog(LogLevel.Warning, "Failed to get item from localStorage", () => Times.Once());
    }

    [Fact]
    public async Task SetItemAsync_ShouldLogException_WhenJSExceptionThrown()
    {
        // Arrange
        const string key = "testKey";
        const string value = "testValue";
        var jsException = new JSException("JS Error");
        _mockJSRuntime.Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.setItem", It.IsAny<object[]>()))
            .ThrowsAsync(jsException);

        // Act
        await _localStorageService.SetItemAsync(key, value);

        // Assert
        _mockLogger.VerifyLog(LogLevel.Warning, "Failed to set item in localStorage", () => Times.Once());
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldLogException_WhenJSExceptionThrown()
    {
        // Arrange
        const string key = "testKey";
        var jsException = new JSException("JS Error");
        _mockJSRuntime.Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.removeItem", It.IsAny<object[]>()))
            .ThrowsAsync(jsException);

        // Act
        await _localStorageService.RemoveItemAsync(key);

        // Assert
        _mockLogger.VerifyLog(LogLevel.Warning, "Failed to remove item from localStorage", () => Times.Once());
    }

    [Fact]
    public async Task ClearAsync_ShouldLogException_WhenJSExceptionThrown()
    {
        // Arrange
        var jsException = new JSException("JS Error");
        _mockJSRuntime.Setup(x => x.InvokeAsync<IJSVoidResult>("localStorage.clear", It.IsAny<object[]>()))
            .ThrowsAsync(jsException);

        // Act
        await _localStorageService.ClearAsync();

        // Assert
        _mockLogger.VerifyLog(LogLevel.Warning, "Failed to clear localStorage", () => Times.Once());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetItemAsync_ShouldThrowArgumentException_WhenKeyIsNullOrWhitespace(string key)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _localStorageService.GetItemAsync(key));
    }

    [Fact]
    public async Task GetItemAsync_ShouldThrowArgumentException_WhenKeyIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _localStorageService.GetItemAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task SetItemAsync_ShouldThrowArgumentException_WhenKeyIsNullOrWhitespace(string key)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _localStorageService.SetItemAsync(key, "value"));
    }

    [Fact]
    public async Task SetItemAsync_ShouldThrowArgumentException_WhenKeyIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _localStorageService.SetItemAsync(null!, "value"));
    }

    [Fact]
    public async Task SetItemAsync_ShouldThrowArgumentNullException_WhenValueIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _localStorageService.SetItemAsync("key", null!));
    }
}
