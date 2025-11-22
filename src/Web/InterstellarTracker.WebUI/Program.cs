using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using InterstellarTracker.WebUI.Components;
using InterstellarTracker.WebUI.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Add HTTP client for Calculation Service
var calculationServiceUrl = builder.Configuration.GetValue<string>("CalculationServiceUrl") ?? "http://localhost:5001";
builder.Services.AddHttpClient<CalculationServiceClient>(client =>
{
    client.BaseAddress = new Uri(calculationServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add health checks with dependency on CalculationService
builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri($"{calculationServiceUrl}/health"), "CalculationService");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Prometheus metrics middleware
app.UseHttpMetrics();

app.UseAntiforgery();

// Map health check endpoint
app.MapHealthChecks("/health");

// Prometheus metrics endpoint
app.MapMetrics();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
