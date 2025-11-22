using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using System.Reflection;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Application layer (MediatR, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure layer (Repositories)
builder.Services.AddInfrastructure();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS for development
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Calculation Service API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

// Prometheus metrics middleware
app.UseHttpMetrics();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Prometheus metrics endpoint
app.MapMetrics();

app.Run();
