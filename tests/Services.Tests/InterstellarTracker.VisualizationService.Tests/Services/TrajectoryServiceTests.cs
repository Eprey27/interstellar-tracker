using FluentAssertions;
using InterstellarTracker.VisualizationService.Models;
using InterstellarTracker.VisualizationService.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InterstellarTracker.VisualizationService.Tests.Services;

/// <summary>
/// Unit tests for TrajectoryService (with mocked CalculationServiceClient)
/// Refactored to use dependency injection of ICalculationServiceClient
/// </summary>
public class TrajectoryServiceTests
{
    private readonly Mock<ILogger<TrajectoryService>> _loggerMock;
    private readonly Mock<ICalculationServiceClient> _calculationServiceClientMock;
    private readonly TrajectoryService _service;

    public TrajectoryServiceTests()
    {
        _loggerMock = new Mock<ILogger<TrajectoryService>>();
        _calculationServiceClientMock = new Mock<ICalculationServiceClient>();
        _service = new TrajectoryService(_loggerMock.Object, _calculationServiceClientMock.Object);
    }

    [Fact]
    public async Task GetTrajectoryAsync_ValidObjectId_ReturnsTrajectory()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var mockPoints = new List<TrajectoryPoint>
        {
            new TrajectoryPoint(
                Timestamp: new DateTime(2019, 12, 8, 0, 0, 0, DateTimeKind.Utc),
                Position: new Vector3D(1.0, 0.0, 0.0),
                Velocity: new Vector3D(0.0, 1.0, 0.0))
        };
        
        _calculationServiceClientMock
            .Setup(x => x.GetTrajectoryAsync(objectId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPoints);

        // Act
        var result = await _service.GetTrajectoryAsync(objectId);

        // Assert
        result.Should().NotBeNull();
        result!.ObjectId.Should().Be(objectId);
        result.Points.Should().NotBeEmpty();
        result.Points.Should().HaveCount(1);
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetTrajectoryAsync_WithDateRange_ReturnsFilteredPoints()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var startDate = new DateTime(2019, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2019, 12, 10, 0, 0, 0, DateTimeKind.Utc);
        
        var mockPoints = new List<TrajectoryPoint>
        {
            new TrajectoryPoint(
                Timestamp: new DateTime(2019, 12, 5, 0, 0, 0, DateTimeKind.Utc),
                Position: new Vector3D(1.0, 0.0, 0.0),
                Velocity: new Vector3D(0.0, 1.0, 0.0))
        };
        
        _calculationServiceClientMock
            .Setup(x => x.GetTrajectoryAsync(objectId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPoints);

        // Act
        var result = await _service.GetTrajectoryAsync(objectId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result!.Points.Should().NotBeEmpty();
        result.Points.Should().OnlyContain(p => p.Timestamp >= startDate && p.Timestamp <= endDate);
        result.Points.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetTrajectoryAsync_InvalidObjectId_ReturnsNull()
    {
        // Arrange
        var objectId = "NonExistent";
        _calculationServiceClientMock
            .Setup(x => x.GetTrajectoryAsync(objectId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<TrajectoryPoint>?)null);

        // Act
        var result = await _service.GetTrajectoryAsync(objectId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTrajectoryAsync_GeneratesRealisticData()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var mockPoints = new List<TrajectoryPoint>
        {
            new TrajectoryPoint(
                Timestamp: new DateTime(2019, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                Position: new Vector3D(1.0, 0.0, 0.0),
                Velocity: new Vector3D(0.0, 1.0, 0.0)),
            new TrajectoryPoint(
                Timestamp: new DateTime(2019, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                Position: new Vector3D(2.0, 0.0, 0.0),
                Velocity: new Vector3D(0.0, 2.0, 0.0))
        };
        
        _calculationServiceClientMock
            .Setup(x => x.GetTrajectoryAsync(objectId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPoints);

        // Act
        var result = await _service.GetTrajectoryAsync(objectId);

        // Assert
        result.Should().NotBeNull();
        var firstPoint = result!.Points.First();
        var lastPoint = result.Points.Last();

        firstPoint.Position.Should().NotBeNull();
        firstPoint.Velocity.Should().NotBeNull();
        result.Points.Should().BeInAscendingOrder(p => p.Timestamp);
    }

    [Fact]
    public async Task GetPositionAsync_ValidObjectIdAndDate_ReturnsPosition()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var date = new DateTime(2019, 12, 8, 0, 0, 0, DateTimeKind.Utc);
        var mockPosition = new Vector3D(1.0, 0.0, 0.0);
        var mockVelocity = new Vector3D(0.0, 1.0, 0.0);
        var mockDistance = 7.249;
        
        _calculationServiceClientMock
            .Setup(x => x.GetPositionAsync(objectId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPosition, mockVelocity, mockDistance));

        // Act
        var result = await _service.GetPositionAsync(objectId, date);

        // Assert
        result.Should().NotBeNull();
        result!.ObjectId.Should().Be(objectId);
        result.Timestamp.Should().Be(date);
        result.Position.Should().NotBeNull();
        result.Velocity.Should().NotBeNull();
        result.DistanceFromSun.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPositionAsync_InvalidObjectId_ReturnsNull()
    {
        // Arrange
        var objectId = "InvalidObject";
        var date = DateTime.UtcNow;
        _calculationServiceClientMock
            .Setup(x => x.GetPositionAsync(objectId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((Vector3D Position, Vector3D Velocity, double Distance)?)null);

        // Act
        var result = await _service.GetPositionAsync(objectId, date);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPositionAsync_CalculatesDistanceCorrectly()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var date = new DateTime(2019, 12, 8, 0, 0, 0, DateTimeKind.Utc);
        var mockPosition = new Vector3D(1.496e8, 0.0, 0.0); // 1 AU in km
        var mockVelocity = new Vector3D(0.0, 32.2, 0.0);
        var mockDistance = 1.0; // 1 AU
        
        _calculationServiceClientMock
            .Setup(x => x.GetPositionAsync(objectId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPosition, mockVelocity, mockDistance));

        // Act
        var result = await _service.GetPositionAsync(objectId, date);

        // Assert
        result.Should().NotBeNull();
        result!.DistanceFromSun.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public async Task GetTrajectoryAsync_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var mockPoints = new List<TrajectoryPoint>
        {
            new TrajectoryPoint(
                Timestamp: DateTime.UtcNow,
                Position: new Vector3D(1.0, 0.0, 0.0),
                Velocity: new Vector3D(0.0, 1.0, 0.0))
        };
        
        _calculationServiceClientMock
            .Setup(x => x.GetTrajectoryAsync(objectId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPoints);
        
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _service.GetTrajectoryAsync(objectId, null, null, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPositionAsync_LogsCorrectInformation()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var date = DateTime.UtcNow;
        _calculationServiceClientMock
            .Setup(x => x.GetPositionAsync(objectId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new Vector3D(1.0, 0.0, 0.0), new Vector3D(0.0, 1.0, 0.0), 7.249));

        // Act
        await _service.GetPositionAsync(objectId, date);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Calculating position")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
