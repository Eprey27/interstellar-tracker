using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});

// Add health checks for downstream services
builder.Services.AddHealthChecks()
    .AddUrlGroup(
        new Uri(builder.Configuration["Services:CalculationService"] ?? "http://localhost:5001/health"),
        name: "calculation-service",
        timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(
        new Uri(builder.Configuration["Services:VisualizationService"] ?? "http://localhost:5002/health"),
        name: "visualization-service",
        timeout: TimeSpan.FromSeconds(5));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

// Prometheus metrics middleware
app.UseHttpMetrics();

// Map YARP reverse proxy
app.MapReverseProxy();

// Health check endpoint
app.MapHealthChecks("/health");

// Prometheus metrics endpoint
app.MapMetrics();

app.Run();
