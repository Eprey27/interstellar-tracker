using Prometheus;
using InterstellarTracker.ApiGateway.Extensions;
using InterstellarTracker.Application;
using InterstellarTracker.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddGatewayTelemetry(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddGatewayCors(builder.Configuration);

var app = builder.Build();

app.UseGatewayMiddleware();
app.MapReverseProxy();
app.MapHealthChecks("/health");
app.MapMetrics();

await app.RunAsync();
