using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using System.Text.Json;

namespace InterstellarTracker.VisualizationService.Tests.Infrastructure;

/// <summary>
/// WireMock server to mock CalculationService HTTP responses in integration tests.
/// Best practice: Use WireMock.Net for HTTP mocking instead of mocking HttpClient.
/// </summary>
public class CalculationServiceMock : IDisposable
{
    private readonly WireMockServer _server;

    public string BaseUrl => _server.Url!;

    public CalculationServiceMock()
    {
        // Start WireMock server on random port
        _server = WireMockServer.Start();
        ConfigureDefaultStubs();
    }

    private void ConfigureDefaultStubs()
    {
        // Mock successful trajectory response for 2I/Borisov
        _server
            .Given(Request.Create()
                .WithPath("/api/calculations/trajectory")
                .WithBody(body => body.Contains("2I/Borisov"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    objectId = "2I/Borisov",
                    points = GenerateMockTrajectoryPoints()
                })));

        // Mock 404 for NonExistent object
        _server
            .Given(Request.Create()
                .WithPath("/api/calculations/trajectory")
                .WithBody(body => body.Contains("NonExistent"))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(404));

        // Mock successful position response for 2I/Borisov
        _server
            .Given(Request.Create()
                .WithPath("/api/calculations/position/*")
                .WithParam("date")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    objectId = "2I/Borisov", // Required by PositionResponseDto
                    position = new { x = 1.0e8, y = 5.0e7, z = 2.0e7 },
                    velocity = new { x = 0.0, y = 32.2, z = 10.5 },
                    distanceFromSun = 7.249
                })));
    }

    private static object[] GenerateMockTrajectoryPoints()
    {
        var baseDate = new DateTime(2019, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        var points = new List<object>();

        // Generate 30 trajectory points (one per day for December 2019)
        for (int i = 0; i < 30; i++)
        {
            var date = baseDate.AddDays(i);
            points.Add(new
            {
                timestamp = date,
                position = new
                {
                    x = 1.0e8 + i * 1e6,
                    y = 5.0e7 + i * 5e5,
                    z = 2.0e7 + i * 2e5
                },
                velocity = new
                {
                    x = 0.0,
                    y = 32.2,
                    z = 10.5
                }
            });
        }

        return points.ToArray();
    }

    public void Dispose()
    {
        _server?.Stop();
        _server?.Dispose();
    }
}
