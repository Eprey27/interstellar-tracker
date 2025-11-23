using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using InterstellarTracker.VisualizationService.Tests.Infrastructure;

namespace InterstellarTracker.VisualizationService.Tests.Controllers;

/// <summary>
/// Integration tests for TrajectoryController with WireMock for CalculationService.
/// Best practice: Use WireMock.Net to mock external HTTP dependencies.
/// </summary>
public class TrajectoryControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TrajectoryControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetTrajectory_ValidObjectId_ReturnsOk()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var encodedObjectId = Uri.EscapeDataString(objectId);

        // Act
        var response = await _client.GetAsync($"/api/trajectories/{encodedObjectId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTrajectory_ValidObjectId_ReturnsTrajectoryData()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var encodedObjectId = Uri.EscapeDataString(objectId);

        // Act
        var response = await _client.GetAsync($"/api/trajectories/{encodedObjectId}");
        var trajectory = await response.Content.ReadFromJsonAsync<TrajectoryResponse>();

        // Assert
        trajectory.Should().NotBeNull();
        trajectory!.ObjectId.Should().Be(objectId);
        trajectory.Points.Should().NotBeEmpty();
        trajectory.Points.Should().HaveCountGreaterThan(10); // Minimum points for useful trajectory
    }

    [Fact]
    public async Task GetTrajectory_WithDateRange_ReturnsFilteredPoints()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var encodedObjectId = Uri.EscapeDataString(objectId);
        var startDate = new DateTime(2019, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await _client.GetAsync($"/api/trajectories/{encodedObjectId}?startDate={startDate:O}&endDate={endDate:O}");
        var trajectory = await response.Content.ReadFromJsonAsync<TrajectoryResponse>();

        // Assert
        trajectory.Should().NotBeNull();
        trajectory!.Points.Should().NotBeEmpty();
        trajectory.Points.Should().OnlyContain(p => p.Timestamp >= startDate && p.Timestamp <= endDate);
    }

    [Fact]
    public async Task GetTrajectory_InvalidObjectId_ReturnsNotFound()
    {
        // Arrange
        var objectId = "NonExistent";
        var encodedObjectId = Uri.EscapeDataString(objectId);

        // Act
        var response = await _client.GetAsync($"/api/trajectories/{encodedObjectId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPosition_ValidObjectIdAndDate_ReturnsOk()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var encodedObjectId = Uri.EscapeDataString(objectId);
        var date = new DateTime(2019, 12, 8, 0, 0, 0, DateTimeKind.Utc); // Perihelion date

        // Act
        var response = await _client.GetAsync($"/api/positions/{encodedObjectId}?date={date:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPosition_ValidObjectIdAndDate_ReturnsPositionData()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var encodedObjectId = Uri.EscapeDataString(objectId);
        var date = new DateTime(2019, 12, 8, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await _client.GetAsync($"/api/positions/{encodedObjectId}?date={date:O}");
        var position = await response.Content.ReadFromJsonAsync<PositionResponse>();

        // Assert
        position.Should().NotBeNull();
        position!.ObjectId.Should().Be(objectId);
        position.Timestamp.Should().Be(date);
        position.Position.Should().NotBeNull();
        position.Position.X.Should().NotBe(0); // Should have valid coordinates
        position.Velocity.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPosition_InvalidDate_ReturnsBadRequest()
    {
        // Arrange
        var objectId = "2I/Borisov";
        var encodedObjectId = Uri.EscapeDataString(objectId);
        var invalidDate = "not-a-date";

        // Act
        var response = await _client.GetAsync($"/api/positions/{encodedObjectId}?date={invalidDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

// DTOs for responses (will move to shared contracts later)
public record TrajectoryResponse(
    string ObjectId,
    List<TrajectoryPoint> Points,
    DateTime GeneratedAt
);

public record TrajectoryPoint(
    DateTime Timestamp,
    Vector3D Position,
    Vector3D Velocity
);

public record PositionResponse(
    string ObjectId,
    DateTime Timestamp,
    Vector3D Position,
    Vector3D Velocity,
    double DistanceFromSun
);

public record Vector3D(
    double X,
    double Y,
    double Z
);
