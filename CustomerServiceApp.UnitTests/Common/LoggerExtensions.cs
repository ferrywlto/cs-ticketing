using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerServiceApp.UnitTests.Common;

public static class LoggerExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, LogLevel logLevel, string message, Func<Times> times)
    {
        mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
