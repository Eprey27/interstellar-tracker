using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterstellarTracker.VisualizationService.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that configures WireMock for CalculationService mocking.
/// Best practice: Override configuration to point HttpClient to WireMock server.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private CalculationServiceMock? _calculationServiceMock;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _calculationServiceMock = new CalculationServiceMock();

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override CalculationService:BaseUrl to point to WireMock
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CalculationService:BaseUrl"] = _calculationServiceMock.BaseUrl
            });
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync()
    {
        _calculationServiceMock?.Dispose();
        return base.DisposeAsync().AsTask();
    }
}
