using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using System.Text.Json;

namespace InterstellarTracker.VisualizationService.Tests.Infrastructure;

/// <summary>
/// WireMock server to mock CalculationService HTTP responses in integration tests.
/// Best practice: Use WireMock.Net for HTTP mocking instead of mocking HttpClient.
/// 
/// FIXED: 
/// - Improved stub matching and ordering (404 must be before 200)
/// - Added explicit status code + body for all responses
/// - Added better debugging with method/path logging
/// - Ensured JSON serialization matches expected format
/// </summary>
public class CalculationServiceMock : IDisposable
{
    private readonly WireMockServer _server;

    public string BaseUrl => _server.Url!;

    public CalculationServiceMock()
    {
        // Start WireMock server on random port
        _server = WireMockServer.Start();
        System.Diagnostics.Debug.WriteLine($"[WireMock] Server started at {_server.Url}");

        ConfigureDefaultStubs();
        System.Diagnostics.Debug.WriteLine($"[WireMock] Stubs configured. Total mappings: {_server.Mappings.Count()}");
    }

    private void ConfigureDefaultStubs()
    {
        // PRIORITY 1: Mock successful trajectory response for 2I/Borisov (POST)
        // Match: POST /api/calculations/trajectory with body containing "2I/Borisov"
        _server!
            .Given(Request.Create()
                .WithPath("/api/calculations/trajectory")
                .UsingPost()
                .WithBody(body => !string.IsNullOrEmpty(body) && body.Contains("2I/Borisov")))
            .AtPriority(1)
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    objectId = "2I/Borisov",
                    points = GenerateMockTrajectoryPoints(),
                    generatedAt = DateTime.UtcNow.ToString("o")
                })));

        // PRIORITY 2: Mock 404 for NonExistent object (POST)
        // Must come AFTER the successful response stub so it doesn't match first
        _server
            .Given(Request.Create()
                .WithPath("/api/calculations/trajectory")
                .UsingPost()
                .WithBody(body => !string.IsNullOrEmpty(body) && body.Contains("NonExistent")))
            .AtPriority(2)
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    error = "Object not found",
                    statusCode = 404
                })));

        // PRIORITY 3: Mock successful position response for 2I/Borisov (GET)
        // Match: GET /api/calculations/position/2I%2FBorisov?date=...
        _server
            .Given(Request.Create()
                .WithPath(p => p.StartsWith("/api/calculations/position/"))
                .UsingGet()
                .WithParam("date"))
            .AtPriority(3)
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    objectId = "2I/Borisov",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    position = new { x = 1.0e8, y = 5.0e7, z = 2.0e7 },
                    velocity = new { x = 0.0, y = 32.2, z = 10.5 },
                    distanceFromSun = 7.249
                })));

        // CATCH-ALL for debugging: Log any unmatched request
        _server
            .Given(Request.Create())
            .AtPriority(999)
            .RespondWith(Response.Create()
                .WithStatusCode(500)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new
                {
                    error = "Unmatched request - no stub matched your request",
                    statusCode = 500
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
                timestamp = date.ToString("o"),
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                _server?.Stop();
                _server?.Dispose();
                System.Diagnostics.Debug.WriteLine("[WireMock] Server stopped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WireMock] Error stopping server: {ex.Message}");
            }
        }
    }

    ~CalculationServiceMock()
    {
        Dispose(false);
    }
}

