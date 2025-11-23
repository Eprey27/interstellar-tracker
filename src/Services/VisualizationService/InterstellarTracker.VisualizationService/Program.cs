using InterstellarTracker.VisualizationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register HTTP client for CalculationService
builder.Services.AddHttpClient<ICalculationServiceClient, CalculationServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CalculationService:BaseUrl"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register application services
builder.Services.AddScoped<ITrajectoryService, TrajectoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Make Program accessible to tests
public partial class Program
{
    /// <summary>
    /// Protected constructor required for integration tests.
    /// This partial class enables test discovery and configuration.
    /// </summary>
    protected Program() { }
}
