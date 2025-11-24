using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using InterstellarTracker.WebUI.Components;
using InterstellarTracker.WebUI.Services;
using InterstellarTracker.WebUI.Extensions;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Add telemetry
builder.Services.AddWebUITelemetry(builder.Configuration);

// Configure Calculation Service HTTP client with externalized URL
builder.Services.AddHttpClient<CalculationServiceClient>(client =>
{
    var calculationServiceUrl = builder.Configuration["Services:CalculationService:Url"]
                             ?? "http://localhost:5001";
    client.BaseAddress = new Uri(calculationServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add health checks
builder.Services.AddHealthChecks();
var app = builder.Build();

// Configure middleware
app.UseWebUIMiddleware();

// Map health check endpoint
app.MapHealthChecks("/health");

// Prometheus metrics endpoint
app.MapMetrics();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
