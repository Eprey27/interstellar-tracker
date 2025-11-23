using FluentAssertions;
using InterstellarTracker.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InterstellarTracker.Application.Tests.Configuration;

/// <summary>
/// Unit tests for <see cref="ServiceConfiguration"/>.
/// Validates that service URLs are loaded correctly from configuration.
/// Ensures RSPEC-1075 compliance: no hardcoded URIs in production code.
/// </summary>
public class ServiceConfigurationTests
{
    [Fact]
    public void GetCalculationServiceUrl_WithValidConfiguration_ReturnsConfiguredUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Services:CalculationService:Url", "https://calc.example.com" }
            })
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act
        var url = serviceConfig.GetCalculationServiceUrl();

        // Assert
        url.Should().Be(new Uri("https://calc.example.com"));
    }

    [Fact]
    public void GetCalculationServiceUrl_WithEnvironmentVariable_ReturnsEnvironmentValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "SERVICES_CALCULATIONSERVICE_URL", "https://calc-env.example.com" }
            })
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act
        var url = serviceConfig.GetCalculationServiceUrl();

        // Assert
        url.Should().Be(new Uri("https://calc-env.example.com"));
    }

    [Fact]
    public void GetCalculationServiceUrl_WithNoConfiguration_UsesDefaultUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act
        var url = serviceConfig.GetCalculationServiceUrl();

        // Assert - should not throw, should return default
        url.Host.Should().Be("localhost");
        url.Port.Should().Be(5001);
    }

    [Fact]
    public void GetVisualizationServiceUrl_WithValidConfiguration_ReturnsConfiguredUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Services:VisualizationService:Url", "https://viz.example.com" }
            })
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act
        var url = serviceConfig.GetVisualizationServiceUrl();

        // Assert
        url.Should().Be(new Uri("https://viz.example.com"));
    }

    [Fact]
    public void GetAuthServiceUrl_WithValidConfiguration_ReturnsConfiguredUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Services:AuthService:Url", "https://auth.example.com" }
            })
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act
        var url = serviceConfig.GetAuthServiceUrl();

        // Assert
        url.Should().Be(new Uri("https://auth.example.com"));
    }

    [Fact]
    public void GetApiGatewayUrl_WithValidConfiguration_ReturnsConfiguredUrl()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Services:ApiGateway:Url", "https://gateway.example.com" }
            })
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act
        var url = serviceConfig.GetApiGatewayUrl();

        // Assert
        url.Should().Be(new Uri("https://gateway.example.com"));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ServiceConfiguration(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetCalculationServiceUrl_WithEmptyUrl_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Services:CalculationService:Url", "" }
            })
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act & Assert
        var act = () => serviceConfig.GetCalculationServiceUrl();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Configuration for CalculationService URL is missing*");
    }

    [Fact]
    public void GetCalculationServiceUrl_WithInvalidUrl_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Services:CalculationService:Url", "not-a-valid-uri" }
            })
            .Build();

        var serviceConfig = new ServiceConfiguration(config);

        // Act & Assert
        var act = () => serviceConfig.GetCalculationServiceUrl();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Configuration for CalculationService URL is invalid*");
    }
}
