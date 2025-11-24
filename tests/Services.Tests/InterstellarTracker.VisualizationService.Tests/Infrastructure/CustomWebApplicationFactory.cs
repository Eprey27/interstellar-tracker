using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using InterstellarTracker.VisualizationService.Services;

namespace InterstellarTracker.VisualizationService.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that configures WireMock for CalculationService mocking.
/// 
/// KEY INSIGHT: The HttpClient BaseAddress is set during app.Build(), before our factory
/// can override configuration. So we use a different strategy:
/// 1. Create the mock server FIRST
/// 2. Override the ICalculationServiceClient registration to use the mock URL
/// 3. This happens AFTER configuration, so it takes precedence
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private CalculationServiceMock? _calculationServiceMock;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Initialize WireMock server
        _calculationServiceMock = new CalculationServiceMock();
        System.Diagnostics.Debug.WriteLine(
            $"[Factory] WireMock initialized at: {_calculationServiceMock.BaseUrl}");

        // CRITICAL: Override services AFTER the app is built
        // This allows us to replace the HttpClient that was created with the wrong URL
        builder.ConfigureServices(services =>
        {
            // Remove existing HttpClient registrations for ICalculationServiceClient
            var httpClientDescriptors = services
                .Where(sd =>
                    sd.ServiceType.Name.Contains("ICalculationServiceClient") ||
                    (sd.ServiceType.IsGenericType &&
                     sd.ServiceType.GetGenericArguments().Any(arg =>
                         arg.Name.Contains("CalculationServiceClient"))))
                .ToList();

            foreach (var descriptor in httpClientDescriptors)
            {
                System.Diagnostics.Debug.WriteLine($"[Factory] Removing: {descriptor.ServiceType.Name}");
                services.Remove(descriptor);
            }

            // Re-register with correct URL
            services.AddHttpClient<ICalculationServiceClient, CalculationServiceClient>(client =>
            {
                var mockUrl = _calculationServiceMock!.BaseUrl;
                System.Diagnostics.Debug.WriteLine(
                    $"[Factory] Re-registering HttpClient with BaseAddress: {mockUrl}");

                client.BaseAddress = new Uri(mockUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            System.Diagnostics.Debug.WriteLine(
                "[Factory] HttpClient service registration updated");
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        _calculationServiceMock?.Dispose();
        await base.DisposeAsync();
    }
}
