# Application Insights Configuration Guide

## Overview

Application Insights provides comprehensive monitoring, diagnostics, and analytics for the Interstellar Tracker services. This guide covers setup, configuration, and usage.

## Azure Setup

### 1. Create Application Insights Resource

**Via Azure Portal:**

1. Navigate to Azure Portal → Create a resource
2. Search for "Application Insights"
3. Click "Create"
4. Configuration:
   - **Subscription**: Your subscription
   - **Resource Group**: interstellar-tracker-rg
   - **Name**: interstellar-tracker-appinsights
   - **Region**: Same as your Container Apps (e.g., West Europe)
   - **Workspace-based**: Yes (select existing Log Analytics workspace or create new)
5. Click "Review + Create" → "Create"

**Via Azure CLI:**

```bash
# Create Log Analytics workspace first
az monitor log-analytics workspace create \
  --resource-group interstellar-tracker-rg \
  --workspace-name interstellar-tracker-workspace \
  --location westeurope

# Create Application Insights
az monitor app-insights component create \
  --app interstellar-tracker-appinsights \
  --location westeurope \
  --resource-group interstellar-tracker-rg \
  --workspace "/subscriptions/{subscription-id}/resourceGroups/interstellar-tracker-rg/providers/Microsoft.OperationalInsights/workspaces/interstellar-tracker-workspace"
```

### 2. Get Connection String

**Via Azure Portal:**

1. Navigate to your Application Insights resource
2. Go to "Overview" blade
3. Copy the **Connection String** (NOT the Instrumentation Key)
   - Format: `InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://...`

**Via Azure CLI:**

```bash
az monitor app-insights component show \
  --app interstellar-tracker-appinsights \
  --resource-group interstellar-tracker-rg \
  --query connectionString \
  --output tsv
```

### 3. Store Connection String Securely

**Option 1: Azure Key Vault (Recommended for Production)**

```bash
# Create Key Vault
az keyvault create \
  --name interstellar-kv \
  --resource-group interstellar-tracker-rg \
  --location westeurope

# Store connection string
az keyvault secret set \
  --vault-name interstellar-kv \
  --name "ApplicationInsights--ConnectionString" \
  --value "InstrumentationKey=...;IngestionEndpoint=..."

# Grant Container Apps access
az keyvault set-policy \
  --name interstellar-kv \
  --object-id <container-app-managed-identity-id> \
  --secret-permissions get list
```

**Option 2: Environment Variables (For Development)**

```bash
# Local .env file (NOT committed to git)
ApplicationInsights__ConnectionString=InstrumentationKey=...;IngestionEndpoint=...
```

**Option 3: User Secrets (Local Development)**

```powershell
# CalculationService
cd src/Services/CalculationService/InterstellarTracker.CalculationService
dotnet user-secrets init
dotnet user-secrets set "ApplicationInsights:ConnectionString" "InstrumentationKey=...;IngestionEndpoint=..."

# WebUI
cd src/Web/InterstellarTracker.WebUI
dotnet user-secrets init
dotnet user-secrets set "ApplicationInsights:ConnectionString" "InstrumentationKey=...;IngestionEndpoint=..."
```

## Configuration

### appsettings.json

Both services already have the configuration structure:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "",
    "EnableAdaptiveSampling": true,
    "EnableQuickPulseMetricStream": true,
    "EnableDependencyTracking": true,
    "EnablePerformanceCounterCollectionModule": true
  }
}
```

**Configuration Options:**

- **ConnectionString**: Azure Application Insights connection string
- **EnableAdaptiveSampling**: Automatically adjusts telemetry volume based on traffic (saves costs)
- **EnableQuickPulseMetricStream**: Live Metrics stream for real-time monitoring
- **EnableDependencyTracking**: Track HTTP calls, SQL queries, etc.
- **EnablePerformanceCounterCollectionModule**: CPU, memory, process metrics

### Program.cs

Already configured in both services:

```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});
```

## What Gets Tracked Automatically

### HTTP Requests

- Request URL, method, status code
- Response time
- Server name
- User agent

### Dependencies

- HTTP calls to CalculationService (from WebUI)
- Database queries (future: when EF Core added)
- Redis cache operations (future)

### Exceptions

- Unhandled exceptions with full stack traces
- Custom exceptions logged via ILogger

### Performance Counters

- CPU usage
- Memory usage
- Process metrics
- GC statistics

### Custom Events

- Page views (Blazor)
- Button clicks (Blazor)
- User actions

## Custom Telemetry

### Track Custom Events

```csharp
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

public class OrbitCalculator
{
    private readonly TelemetryClient _telemetryClient;

    public OrbitCalculator(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public async Task<Position> CalculatePositionAsync(string bodyId, DateTimeOffset date)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var position = await PerformCalculation(bodyId, date);

            // Track successful calculation
            _telemetryClient.TrackEvent("OrbitCalculationCompleted",
                properties: new Dictionary<string, string>
                {
                    { "CelestialBody", bodyId },
                    { "TargetDate", date.ToString("O") }
                },
                metrics: new Dictionary<string, double>
                {
                    { "CalculationTimeMs", stopwatch.ElapsedMilliseconds },
                    { "DistanceFromSunAU", position.Magnitude }
                });

            return position;
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex,
                properties: new Dictionary<string, string>
                {
                    { "CelestialBody", bodyId },
                    { "TargetDate", date.ToString("O") }
                });
            throw;
        }
    }
}
```

### Track Custom Metrics

```csharp
// Track orbital eccentricity distribution
_telemetryClient.TrackMetric("OrbitEccentricity", orbit.Eccentricity,
    properties: new Dictionary<string, string>
    {
        { "ObjectType", "Interstellar" },
        { "ObjectName", "2I/Borisov" }
    });

// Track cache hit ratio
_telemetryClient.TrackMetric("CacheHitRatio", hitRatio,
    count: totalRequests,
    min: 0.0,
    max: 1.0,
    standardDeviation: 0.15);
```

### Track Dependencies

```csharp
// Track external API call
var dependencyStartTime = DateTimeOffset.UtcNow;
var stopwatch = Stopwatch.StartNew();

try
{
    var response = await httpClient.GetAsync("https://ssd.jpl.nasa.gov/api/...");
    
    _telemetryClient.TrackDependency(
        dependencyTypeName: "HTTP",
        dependencyName: "JPL Horizons API",
        data: "GET /api/...",
        startTime: dependencyStartTime,
        duration: stopwatch.Elapsed,
        success: response.IsSuccessStatusCode);
}
catch (Exception ex)
{
    _telemetryClient.TrackDependency(
        dependencyTypeName: "HTTP",
        dependencyName: "JPL Horizons API",
        data: "GET /api/...",
        startTime: dependencyStartTime,
        duration: stopwatch.Elapsed,
        success: false);
    throw;
}
```

### Correlation with Operation ID

Application Insights automatically tracks correlation across services:

```csharp
// In WebUI - initiates request
var position = await _calculationServiceClient.GetPositionAsync("earth", date);
// Operation ID: abc123

// In CalculationService - receives request
// Operation ID: abc123 (same!)
// Parent ID: WebUI request ID

// Both requests appear in the same transaction in Application Insights
```

## Viewing Telemetry in Azure

### Live Metrics Stream

1. Navigate to Application Insights resource
2. Click "Live Metrics"
3. See real-time:
   - Incoming request rate
   - Request duration
   - Failure rate
   - Server metrics (CPU, memory)
   - Sample requests and dependencies

### Application Map

Shows topology of services:

```
WebUI → CalculationService
  ↓           ↓
Cache    PostgreSQL
```

1. Navigate to "Application Map"
2. See:
   - Service dependencies
   - Failure rates per service
   - Average response times
   - Request volumes

### Transaction Search

Find specific requests:

1. Navigate to "Transaction search"
2. Filter by:
   - Time range
   - Event type (Request, Exception, etc.)
   - Operation name
   - Response code
   - Custom properties
3. Click request to see:
   - End-to-end transaction trace
   - All related dependencies
   - Timeline visualization

### Failures

1. Navigate to "Failures"
2. See:
   - Exception counts
   - Failed dependencies
   - Top exceptions by type
   - Affected users
3. Click exception to see full stack trace and context

### Performance

1. Navigate to "Performance"
2. See:
   - Slowest operations
   - Response time percentiles (50th, 95th, 99th)
   - Dependency performance
   - SQL query performance (future)

### Logs (Kusto Queries)

Run powerful queries against telemetry:

```kusto
// Average response time per endpoint
requests
| where timestamp > ago(1h)
| summarize avg(duration) by name
| order by avg_duration desc

// Failed requests with details
requests
| where timestamp > ago(24h)
| where success == false
| project timestamp, name, resultCode, duration, customDimensions
| order by timestamp desc

// Orbital calculation performance
customEvents
| where name == "OrbitCalculationCompleted"
| extend body = tostring(customDimensions.CelestialBody)
| extend calcTime = todouble(customMeasurements.CalculationTimeMs)
| summarize avg(calcTime), p95 = percentile(calcTime, 95) by body
| order by avg_calcTime desc

// Dependency failures
dependencies
| where timestamp > ago(1h)
| where success == false
| summarize count() by name, resultCode
| order by count_ desc

// Exception rate over time
exceptions
| where timestamp > ago(24h)
| summarize count() by bin(timestamp, 1h), type
| render timechart
```

## Alerts

### Create Smart Detection Rules

Navigate to Application Insights → Alerts → Create alert rule:

**1. Slow Response Time**

```kusto
requests
| where timestamp > ago(5m)
| summarize avg_duration = avg(duration), count() by bin(timestamp, 1m)
| where avg_duration > 1000 // > 1 second
```

**2. High Failure Rate**

```kusto
requests
| where timestamp > ago(5m)
| summarize total = count(), failures = countif(success == false)
| extend failure_rate = (failures * 100.0) / total
| where failure_rate > 5 // > 5% failures
```

**3. Dependency Failures**

```kusto
dependencies
| where timestamp > ago(5m)
| where name contains "CalculationService"
| summarize failures = countif(success == false)
| where failures > 10
```

### Action Groups

Create action groups for notifications:

- Email to dev team
- SMS to on-call
- Webhook to Slack/Teams
- Azure Function for auto-remediation

## Best Practices

### 1. Use Sampling for Cost Control

Adaptive sampling is enabled by default. For high-traffic apps:

```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.EnableAdaptiveSampling = true;
});

// Configure sampling rate
builder.Services.Configure<TelemetryConfiguration>(config =>
{
    var adaptiveSamplingSettings = new SamplingPercentageEstimatorSettings
    {
        MaxTelemetryItemsPerSecond = 5 // Adjust based on budget
    };
    config.DefaultTelemetrySink.TelemetryProcessorChainBuilder
        .UseAdaptiveSampling(adaptiveSamplingSettings)
        .Build();
});
```

### 2. Add Custom Properties to All Telemetry

```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.Configure<TelemetryConfiguration>(config =>
{
    config.TelemetryInitializers.Add(new CustomTelemetryInitializer());
});

// CustomTelemetryInitializer.cs
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.GlobalProperties["Environment"] = "Production";
        telemetry.Context.GlobalProperties["ServiceName"] = "CalculationService";
        telemetry.Context.GlobalProperties["Version"] = "1.0.0";
    }
}
```

### 3. Use ILogger for Structured Logging

Application Insights automatically captures ILogger logs:

```csharp
_logger.LogInformation("Calculating position for {CelestialBody} at {Date}", 
    bodyId, date);
// Automatically appears in Application Insights with structured properties
```

### 4. Track Business Metrics

```csharp
// Track user engagement
_telemetryClient.TrackMetric("DailyActiveUsers", uniqueUsers);
_telemetryClient.TrackMetric("QueriesPerUser", avgQueries);

// Track domain-specific metrics
_telemetryClient.TrackMetric("InterstellarObjectsTracked", count);
_telemetryClient.TrackMetric("AverageOrbitalPeriodDays", avgPeriod);
```

## Cost Optimization

### Pricing Tiers

- **First 5 GB/month**: Free
- **Beyond 5 GB**: ~$2.30/GB
- **Data retention**: 90 days free, $0.12/GB/month for longer

### Reduce Costs

1. **Enable Adaptive Sampling**: Done by default
2. **Filter Telemetry**:

   ```csharp
   builder.Services.Configure<TelemetryConfiguration>(config =>
   {
       config.TelemetryProcessorChainBuilder
           .Use(next => new FilterHealthChecksTelemetryProcessor(next))
           .Build();
   });

   public class FilterHealthChecksTelemetryProcessor : ITelemetryProcessor
   {
       public void Process(ITelemetry item)
       {
           if (item is RequestTelemetry request && 
               request.Url.AbsolutePath.Contains("/health"))
           {
               return; // Don't track health checks
           }
           _next.Process(item);
       }
   }
   ```

3. **Set Data Retention**: 30 days instead of 90
4. **Use Log Analytics Workspace**: Separate billing, better querying

## Troubleshooting

### No Telemetry Appearing

1. **Check Connection String**: `dotnet user-secrets list`
2. **Verify Package**: `Microsoft.ApplicationInsights.AspNetCore` 2.23.0+
3. **Check Logs**: Look for Application Insights initialization messages
4. **Test Locally**: Telemetry appears in Output window (Debug mode)
5. **Wait**: Can take 2-5 minutes for first data to appear in Azure

### Missing Dependencies

- Ensure `EnableDependencyTracking = true`
- Check that HttpClient is created via DI (not `new HttpClient()`)

### High Costs

- Review sampling rate
- Filter health check requests
- Reduce data retention period
- Use Log Analytics workspace for cheaper storage

## References

- [Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [.NET SDK Reference](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)
- [Kusto Query Language](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)
- [Pricing Calculator](https://azure.microsoft.com/en-us/pricing/details/monitor/)
- [Best Practices](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-best-practices)
