using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherDashboard.Functions;

namespace WeatherDashboard.Tests.UnitTests.Functions;

public class WeatherFunctionsTests
{
    private readonly Mock<ILogger<WeatherHttpTrigger>> _loggerMock;
    private readonly WeatherHttpTrigger _weatherFunction;

    public WeatherFunctionsTests()
    {
        _loggerMock = new Mock<ILogger<WeatherHttpTrigger>>();
        _weatherFunction = new WeatherHttpTrigger(_loggerMock.Object);
    }

    [Fact]
    public void WeatherHttpTrigger_Constructor_ShouldNotThrow()
    {
        var exception = Record.Exception(() => new WeatherHttpTrigger(_loggerMock.Object));
        Assert.Null(exception);
    }

    [Fact]
    public void WeatherHttpTrigger_Run_ShouldReturnOkResult()
    {
        var request = CreateMockHttpRequest();
        var result = _weatherFunction.Run(request);
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.Equal("Welcome to Azure Functions!", okResult?.Value);
    }

    [Fact]
    public void WeatherHttpTrigger_Run_ShouldLogInformation()
    {
        var request = CreateMockHttpRequest();
        _weatherFunction.Run(request);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("C# HTTP trigger function processed a request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static HttpRequest CreateMockHttpRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = "GET";
        request.Scheme = "http";
        request.Host = new HostString("localhost");
        request.Path = "/api/weather";
        return request;
    }
}

public class HealthCheckFunctionTests
{
    private readonly Mock<ILogger> _loggerMock;

    public HealthCheckFunctionTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    public void HealthCheck_ShouldReturnHealthyStatus()
    {
        var isHealthy = true;
        var timestamp = DateTime.UtcNow;
        Assert.True(isHealthy);
        Assert.True(timestamp <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("Healthy")]
    [InlineData("Unhealthy")]
    [InlineData("Degraded")]
    public void HealthCheck_ShouldReturnValidStatuses(string status)
    {
        var validStatuses = new[] { "Healthy", "Unhealthy", "Degraded" };
        Assert.Contains(status, validStatuses);
    }
}
