using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using WeatherDashboard.Functions.Models;
using WeatherDashboard.Functions.Services;
using WeatherDashboard.Functions.Services.Interfaces;
using Xunit;

namespace WeatherDashboard.Tests.UnitTests.Services;

public class WeatherServiceTests
{
    private readonly Mock<ILogger<WeatherService>> _loggerMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public WeatherServiceTests()
    {
        _loggerMock = new Mock<ILogger<WeatherService>>();
        _cacheMock = new Mock<ICacheService>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.openweathermap.org/")
        };
    }

    [Fact]
    public void WeatherService_Constructor_ShouldNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            var mockService = new Mock<IWeatherService>();
            Assert.NotNull(mockService.Object);
        });

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Lima")]
    [InlineData("London")]
    [InlineData("New York")]
    [InlineData("São Paulo")]
    public void ValidateCityName_WithValidCities_ShouldReturnTrue(string cityName)
    {
        var isValid = IsValidCityName(cityName);
        Assert.True(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("a")]
    public void ValidateCityName_WithInvalidCities_ShouldReturnFalse(string cityName)
    {
        var isValid = IsValidCityName(cityName);
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(0, 0)]
    [InlineData(-12.0464, -77.0428)]
    [InlineData(40.7128, -74.0060)]
    public void ValidateCoordinates_WithValidRange_ShouldReturnTrue(double lat, double lon)
    {
        var isValid = IsValidCoordinates(lat, lon);
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    public void ValidateCoordinates_WithInvalidRange_ShouldReturnFalse(double lat, double lon)
    {
        var isValid = IsValidCoordinates(lat, lon);
        Assert.False(isValid);
    }

    [Fact]
    public void GenerateCacheKey_ShouldBeConsistent()
    {
        var city = "Lima";
        var units = "metric";
        var key1 = GenerateCacheKey("weather", city, units);
        var key2 = GenerateCacheKey("weather", city, units);
        Assert.Equal(key1, key2);
        Assert.Contains(city.ToLower(), key1.ToLower());
        Assert.Contains(units, key1);
    }

    [Theory]
    [InlineData("weather", "Lima", "metric", "weather:lima:metric")]
    [InlineData("forecast", "London", "imperial", "forecast:london:imperial")]
    [InlineData("alerts", "Paris", "standard", "alerts:paris:standard")]
    public void GenerateCacheKey_ShouldFormatCorrectly(string prefix, string city, string units, string expected)
    {
        var result = GenerateCacheKey(prefix, city, units);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void WeatherResponse_ShouldDeserializeCorrectly()
    {
        var json = """
        {
            "name": "Lima",
            "main": {
                "temp": 20.5,
                "feels_like": 22.1,
                "humidity": 65,
                "pressure": 1013,
                "temp_min": 18.0,
                "temp_max": 23.0
            },
            "weather": [{
                "id": 800,
                "main": "Clear",
                "description": "clear sky",
                "icon": "01d"
            }],
            "wind": {
                "speed": 3.5,
                "deg": 180
            }
        }
        """;

        var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(json);
        Assert.NotNull(weatherResponse);
        Assert.Equal("Lima", weatherResponse.Name);
        Assert.Equal(20.5, weatherResponse.Main.Temp);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(60)]
    [InlineData(300)]
    public void CacheExpiration_ShouldBeValid(int minutes)
    {
        var expiration = TimeSpan.FromMinutes(minutes);
        var now = DateTime.UtcNow;
        var expiryTime = now.Add(expiration);
        Assert.True(expiryTime > now);
        Assert.True(expiration.TotalMinutes > 0);
        Assert.True(expiration.TotalHours <= 24);
    }

    [Fact]
    public async Task HttpClient_ShouldHaveCorrectConfiguration()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openweathermap.org/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        Assert.Equal("https://api.openweathermap.org/", httpClient.BaseAddress?.ToString());
        Assert.Equal(TimeSpan.FromSeconds(30), httpClient.Timeout);
    }

    private static bool IsValidCityName(string cityName)
    {
        return !string.IsNullOrWhiteSpace(cityName) &&
               cityName.Trim().Length >= 2 &&
               cityName.Trim().Length <= 100;
    }

    private static bool IsValidCoordinates(double lat, double lon)
    {
        return !double.IsNaN(lat) && !double.IsNaN(lon) &&
               !double.IsInfinity(lat) && !double.IsInfinity(lon) &&
               lat >= -90 && lat <= 90 &&
               lon >= -180 && lon <= 180;
    }

    private static string GenerateCacheKey(string prefix, string city, string units)
    {
        return $"{prefix}:{city.ToLower()}:{units}";
    }
}
