using Prometheus;
using InterstellarTracker.ApiGateway.Extensions;
using InterstellarTracker.Application;
using InterstellarTracker.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add application services
builder.Services.AddApplication();

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add telemetry (Application Insights)
builder.Services.AddGatewayTelemetry(builder.Configuration);

// Register health checks
builder.Services.AddHealthChecks();

// Add CORS with externalized policy
builder.Services.AddGatewayCors(builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseGatewayMiddleware();

// Map YARP reverse proxy
app.MapReverseProxy();

// Health check endpoint
app.MapHealthChecks("/health");

// Prometheus metrics endpoint
app.MapMetrics();

await app.RunAsync();
