using InterstellarTracker.Application;
using InterstellarTracker.Infrastructure;
using InterstellarTracker.WebUI.Components;
using InterstellarTracker.WebUI.Services;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
