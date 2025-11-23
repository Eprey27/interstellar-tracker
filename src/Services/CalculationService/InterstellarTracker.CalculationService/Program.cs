using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using InterstellarTracker.CalculationService.Extensions;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCalculationServiceTelemetry(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddCalculationServiceCors(builder.Configuration);

var app = builder.Build();

app.UseCalculationServiceMiddleware();
app.UseHttpMetrics();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapMetrics();

await app.RunAsync();
