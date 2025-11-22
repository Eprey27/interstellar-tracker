# Monitoring & Observability Guide

## Overview

Interstellar Tracker implements a comprehensive monitoring and observability strategy with both local and cloud-based solutions.

## Local Monitoring Stack

### Prometheus (Metrics Collection)

**URL**: <http://localhost:9090>

Prometheus scrapes metrics from all services every 15 seconds.

**Configuration**: `docker/prometheus/prometheus.yml`

**Monitored Services**:

- CalculationService (port 5001)
- WebUI (port 5000)
- Keycloak (port 8080)
- Prometheus itself (self-monitoring)

**Key Metrics**:

- `http_requests_received_total` - Total HTTP requests per service
- `http_request_duration_seconds` - Request duration histograms
- `process_working_set_bytes` - Memory usage
- `process_cpu_seconds_total` - CPU usage
- Custom application metrics (orbital calculations, cache hits, etc.)

**Useful Queries**:

```promql
# Request rate (requests/second)
rate(http_requests_received_total[5m])

# Average response time
rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])

# 95th percentile response time
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Memory usage trend
process_working_set_bytes
```

### Grafana (Visualization)

**URL**: <http://localhost:3000>
**Credentials**: admin / admin

**Pre-configured Dashboards**:

- **Interstellar Tracker - Overview**: System-wide metrics
  - Request rates per service
  - Average response times
  - Memory usage trends
  - Error rates

**Creating Custom Dashboards**:

1. Navigate to Dashboards → New Dashboard
2. Add Panel → Select Prometheus as data source
3. Enter PromQL query
4. Configure visualization (graph, gauge, table, etc.)
5. Save dashboard

**Dashboard JSON**: `docker/grafana/provisioning/dashboards/interstellar-overview.json`

## Health Checks

All services expose `/health` endpoints for monitoring availability.

### CalculationService

**Endpoint**: <http://localhost:5001/health>

**Checks**:

- Service is running
- Can access in-memory repositories
- Ready to accept requests

### WebUI

**Endpoint**: <http://localhost:5000/health>

**Checks**:

- Service is running
- Can reach CalculationService
- Blazor Server is operational

### Docker Compose Health Checks

```yaml
healthcheck:
  test: ["CMD", "curl", "--fail", "http://localhost:8080/health"]
  interval: 30s
  timeout: 5s
  retries: 3
```

### Kubernetes Readiness/Liveness Probes

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 30

readinessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 10
```

## Metrics Endpoints

### Prometheus Metrics Format

**CalculationService**: <http://localhost:5001/metrics>
**WebUI**: <http://localhost:5000/metrics>

**Sample Output**:

```
# HELP http_requests_received_total Total number of HTTP requests received
# TYPE http_requests_received_total counter
http_requests_received_total{method="GET",controller="CelestialBodies",action="GetAll"} 142

# HELP http_request_duration_seconds HTTP request duration in seconds
# TYPE http_request_duration_seconds histogram
http_request_duration_seconds_bucket{method="GET",le="0.005"} 98
http_request_duration_seconds_bucket{method="GET",le="0.01"} 132
http_request_duration_seconds_bucket{method="GET",le="0.025"} 140
http_request_duration_seconds_bucket{method="GET",le="0.05"} 142
```

## Cloud Monitoring (Azure)

### Application Insights (Coming Soon)

**Features**:

- Distributed tracing across microservices
- Custom telemetry for orbital calculations
- Performance profiling
- Exception tracking
- User analytics
- Availability monitoring

**Configuration**:

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-from-azure-portal"
  }
}
```

**Custom Metrics Example**:

```csharp
_telemetryClient.TrackMetric("OrbitCalculationTime", elapsedMs);
_telemetryClient.TrackDependency("CalculationService", "/api/position", startTime, duration, success);
```

## Alerting (Future)

### Prometheus AlertManager

**Configuration**: `docker/prometheus/alerts.yml`

**Sample Alert**:

```yaml
groups:
  - name: interstellar_alerts
    interval: 30s
    rules:
      - alert: HighResponseTime
        expr: rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m]) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High response time detected"
          description: "Service {{ $labels.service }} has avg response time > 1s"
      
      - alert: ServiceDown
        expr: up{job=~"calculation-service|webui"} == 0
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "Service is down"
          description: "{{ $labels.job }} has been down for more than 2 minutes"
```

## Logging

### Console Logging (Development)

All services log to stdout/stderr using ASP.NET Core logging.

**Log Levels**:

- Trace: Detailed diagnostic information
- Debug: Development-time debugging
- Information: General informational messages
- Warning: Unexpected but recoverable issues
- Error: Error conditions
- Critical: Critical failures requiring immediate attention

**Configuration**: `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "InterstellarTracker": "Debug"
    }
  }
}
```

### Structured Logging with Serilog (Future)

```csharp
Log.Information("Calculating position for {CelestialBody} at {Date}", bodyName, date);
Log.Error(ex, "Failed to retrieve celestial body {BodyId}", bodyId);
```

## Accessing Monitoring Services

### Local Development

| Service | URL | Credentials |
|---------|-----|-------------|
| Prometheus | <http://localhost:9090> | None |
| Grafana | <http://localhost:3000> | admin/admin |
| CalculationService Health | <http://localhost:5001/health> | None |
| CalculationService Metrics | <http://localhost:5001/metrics> | None |
| WebUI Health | <http://localhost:5000/health> | None |
| WebUI Metrics | <http://localhost:5000/metrics> | None |
| Keycloak Admin | <http://localhost:8080> | admin/admin |

### Starting Monitoring Stack

```powershell
# Start all services including Prometheus and Grafana
docker-compose up -d

# View logs
docker-compose logs -f prometheus grafana

# Stop monitoring stack
docker-compose down
```

## Troubleshooting

### Prometheus Not Scraping Targets

1. Check Prometheus targets page: <http://localhost:9090/targets>
2. Verify services are running: `docker-compose ps`
3. Check service metrics endpoints are accessible
4. Review Prometheus configuration: `docker/prometheus/prometheus.yml`

### Grafana Dashboard Not Showing Data

1. Verify Prometheus datasource is configured: Configuration → Data Sources
2. Test PromQL query in Prometheus UI first
3. Check time range selector in Grafana
4. Ensure services are generating metrics

### Health Check Failing

1. Check service logs: `docker-compose logs [service-name]`
2. Verify health endpoint manually: `curl http://localhost:5001/health`
3. Ensure dependencies are running (e.g., CalculationService for WebUI health check)

## Best Practices

1. **Monitor the Right Metrics**: Focus on RED (Rate, Errors, Duration) metrics
2. **Set Baselines**: Establish normal operating ranges for key metrics
3. **Create Actionable Alerts**: Avoid alert fatigue with meaningful thresholds
4. **Use Structured Logging**: Include context in log messages for better debugging
5. **Correlate Logs and Metrics**: Use trace IDs to connect logs across services
6. **Regular Review**: Review dashboards weekly to spot trends

## References

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [prometheus-net Library](https://github.com/prometheus-net/prometheus-net)
- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
