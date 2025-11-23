using FluentAssertions;
using InterstellarTracker.VisualizationService.Models;
using InterstellarTracker.VisualizationService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace InterstellarTracker.VisualizationService.Tests.Services;

/// <summary>
/// Unit tests for CalculationServiceClient - TDD GREEN PHASE
/// 
/// Tests verify HTTP client behavior using mocked HttpMessageHandler
/// to avoid dependency on real CalculationService during unit tests.
/// </summary>
public class CalculationServiceClientTests
{
    private readonly Mock<ILogger<CalculationServiceClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public CalculationServiceClientTests()
    {
        _mockLogger = new Mock<ILogger<CalculationServiceClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    }

    private CalculationServiceClient CreateClient()
    {
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };
        return new CalculationServiceClient(httpClient, _mockLogger.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? content = null)
    {
        var responseMessage = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            responseMessage.Content = new StringContent(
                JsonSerializer.Serialize(content),
                System.Text.Encoding.UTF8,
                "application/json");
        }

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }

    #region GREEN PHASE - Tests for real HTTP behavior

    [Fact]
    public async Task GetTrajectoryAsync_SuccessfulResponse_ReturnsTrajectoryPoints()
    {
        // Arrange
        var mockResponse = new
        {
            ObjectId = "2I/Borisov",
            Points = new[]
            {
                new
                {
                    Timestamp = "2019-12-08T00:00:00Z",
                    Position = new { X = 7.249, Y = 0.0, Z = 0.0 },
                    Velocity = new { X = 0.0, Y = 32.2, Z = 0.0 }
                }
            },
            GeneratedAt = "2025-11-23T10:00:00Z"
        };
        SetupHttpResponse(HttpStatusCode.OK, mockResponse);
        var client = CreateClient();

        // Act
        var result = await client.GetTrajectoryAsync("2I/Borisov", null, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Position.X.Should().Be(7.249);
    }

    [Fact]
    public async Task GetTrajectoryAsync_ObjectNotFound_ReturnsNull()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound);
        var client = CreateClient();

        // Act
        var result = await client.GetTrajectoryAsync("UnknownObject", null, null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPositionAsync_SuccessfulResponse_ReturnsPositionData()
    {
        // Arrange
        var mockResponse = new
        {
            ObjectId = "2I/Borisov",
            Timestamp = "2019-12-08T00:00:00Z",
            Position = new { X = 7.249, Y = 0.0, Z = 0.0 },
            Velocity = new { X = 0.0, Y = 32.2, Z = 0.0 },
            DistanceFromSun = 7.249
        };
        SetupHttpResponse(HttpStatusCode.OK, mockResponse);
        var client = CreateClient();

        // Act
        var result = await client.GetPositionAsync("2I/Borisov", new DateTime(2019, 12, 8));

        // Assert
        result.Should().NotBeNull();
        result!.Value.Position.X.Should().Be(7.249);
        result.Value.Distance.Should().Be(7.249);
    }

    [Fact]
    public async Task GetPositionAsync_ObjectNotFound_ReturnsNull()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound);
        var client = CreateClient();

        // Act
        var result = await client.GetPositionAsync("UnknownObject", new DateTime(2019, 12, 8));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTrajectoryAsync_ServiceUnavailable_ThrowsHttpRequestException()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.ServiceUnavailable);
        var client = CreateClient();

        // Act
        var act = async () => await client.GetTrajectoryAsync("2I/Borisov", null, null);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };
        var logger = _mockLogger.Object;

        // Act
        var client = new CalculationServiceClient(httpClient, logger);

        // Assert
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<ICalculationServiceClient>();
    }

    #endregion

    // TODO REFACTOR PHASE: Add tests for:
    // - Retry logic with Polly (transient failures)
    // - Circuit breaker (consecutive failures)
    // - Caching behavior (hit/miss/expiration)
    // - Timeout handling
    // - Invalid JSON deserialization
}
