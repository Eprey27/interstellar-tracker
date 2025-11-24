using InterstellarTracker.Application.CelestialBodies.Queries.GetCelestialBodyPosition;
using InterstellarTracker.Application.Common.Interfaces;
using InterstellarTracker.Domain.Entities;
using InterstellarTracker.Domain.ValueObjects;
using Moq;

namespace InterstellarTracker.Application.Tests.CelestialBodies.Queries;

/// <summary>
/// Unit tests for GetCelestialBodyPositionQueryHandler.
/// </summary>
public class GetCelestialBodyPositionQueryHandlerTests
{
    private const double AU = 1.496e11; // Astronomical Unit in meters
    private const double SunGM = 1.32712440018e20; // Sun's gravitational parameter

    [Fact]
    public async Task Handle_ValidBodyId_ReturnsSuccessWithPosition()
    {
        // Arrange
        var bodyId = "earth";
        var julianDate = 2451545.0; // J2000 epoch

        var orbitalElements = CreateEarthLikeOrbit();
        var body = new CelestialBody(
            bodyId,
            "Earth",
            CelestialBodyType.Planet,
            5.972e24,
            6.371e6,
            orbitalElements,
            VisualProperties.Planet(RgbColor.Blue, 0.3)
        );

        var mockRepo = new Mock<ICelestialBodyRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(bodyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(body);

        var handler = new GetCelestialBodyPositionQueryHandler(mockRepo.Object);
        var query = new GetCelestialBodyPositionQuery(bodyId, julianDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Magnitude > 0);
    }

    [Fact]
    public async Task Handle_InterstellarObjectId_ReturnsSuccessWithPosition()
    {
        // Arrange
        var borisov = InterstellarObject.CreateBorisov();
        var julianDate = 2458826.5; // Epoch date

        var mockRepo = new Mock<ICelestialBodyRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(borisov.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CelestialBody?)null); // Not found as regular body
        mockRepo.Setup(r => r.GetInterstellarObjectByIdAsync(borisov.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(borisov);

        var handler = new GetCelestialBodyPositionQueryHandler(mockRepo.Object);
        var query = new GetCelestialBodyPositionQuery(borisov.Id, julianDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Magnitude > 0);
        Assert.NotEqual(double.NaN, result.Value.X);
        Assert.NotEqual(double.NaN, result.Value.Y);
        Assert.NotEqual(double.NaN, result.Value.Z);
    }

    [Fact]
    public async Task Handle_NonExistentBodyId_ReturnsFailure()
    {
        // Arrange
        var bodyId = "non-existent";
        var julianDate = 2451545.0;

        var mockRepo = new Mock<ICelestialBodyRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(bodyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CelestialBody?)null);
        mockRepo.Setup(r => r.GetInterstellarObjectByIdAsync(bodyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InterstellarObject?)null);

        var handler = new GetCelestialBodyPositionQueryHandler(mockRepo.Object);
        var query = new GetCelestialBodyPositionQuery(bodyId, julianDate);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task Handle_DifferentJulianDates_ReturnsDifferentPositions()
    {
        // Arrange
        var bodyId = "mars";
        var orbitalElements = CreateMarsLikeOrbit();
        var body = new CelestialBody(
            bodyId,
            "Mars",
            CelestialBodyType.Planet,
            6.4171e23,
            3.3895e6,
            orbitalElements,
            VisualProperties.Planet(RgbColor.Red, 0.15)
        );

        var mockRepo = new Mock<ICelestialBodyRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(bodyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(body);

        var handler = new GetCelestialBodyPositionQueryHandler(mockRepo.Object);

        var query1 = new GetCelestialBodyPositionQuery(bodyId, 2451545.0);
        var query2 = new GetCelestialBodyPositionQuery(bodyId, 2451545.0 + 100); // 100 days later

        // Act
        var result1 = await handler.Handle(query1, CancellationToken.None);
        var result2 = await handler.Handle(query2, CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Value, result2.Value); // Positions should differ
    }

    private static OrbitalElements CreateEarthLikeOrbit()
    {
        return new OrbitalElements(
            semiMajorAxisMeters: AU,
            eccentricity: 0.0167,
            inclinationRadians: 0.0,
            longitudeOfAscendingNodeRadians: 0.0,
            argumentOfPeriapsisRadians: 0.0,
            meanAnomalyAtEpochRadians: 0.0,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );
    }

    private static OrbitalElements CreateMarsLikeOrbit()
    {
        return new OrbitalElements(
            semiMajorAxisMeters: 1.524 * AU,
            eccentricity: 0.0934,
            inclinationRadians: 0.0323,
            longitudeOfAscendingNodeRadians: 0.865,
            argumentOfPeriapsisRadians: 5.865,
            meanAnomalyAtEpochRadians: 0.338,
            epochJulianDate: 2451545.0,
            gravitationalParameter: SunGM
        );
    }
}
