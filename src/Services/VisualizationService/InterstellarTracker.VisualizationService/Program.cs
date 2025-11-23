using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using InterstellarTracker.VisualizationService.Services;
using InterstellarTracker.VisualizationService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add core services
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddSwaggerGen();

// Register HTTP client for CalculationService with externalized configuration
builder.Services.AddHttpClient<ICalculationServiceClient, CalculationServiceClient>(client =>
{
    var baseUrl = builder.Configuration["Services:CalculationService:Url"]
                ?? builder.Configuration["CalculationService:BaseUrl"]
                ?? "http://localhost:5001";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register application services
builder.Services.AddScoped<ITrajectoryService, TrajectoryService>();

// Add telemetry
builder.Services.AddVisualizationServiceTelemetry(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS
builder.Services.AddVisualizationServiceCors(builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseVisualizationServiceMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

// Make Program accessible to tests
public partial class Program
{
    /// <summary>
    /// Protected constructor required for integration tests.
    /// This partial class enables test discovery and configuration.
    /// </summary>
    protected Program() { }
}
