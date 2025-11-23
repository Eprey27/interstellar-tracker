using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using InterstellarTracker.CalculationService.Extensions;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add core services
builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add telemetry (Application Insights)
builder.Services.AddCalculationServiceTelemetry(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS with externalized policy
builder.Services.AddCalculationServiceCors(builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseCalculationServiceMiddleware();

// Prometheus metrics middleware
app.UseHttpMetrics();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Prometheus metrics endpoint
app.MapMetrics();

await app.RunAsync();
