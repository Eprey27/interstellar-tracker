# ADR-007: CalculationService Integration - TDD Approach

**Status**: In Progress  
**Date**: 2025-11-23  
**Deciders**: Development Team  
**Context**: Replacing mock trajectory calculations with real CalculationService HTTP integration

---

## 📋 PHASE 1: REQUIREMENTS & DESIGN

### Business Requirements

**FR-001**: VisualizationService must retrieve trajectory calculations from CalculationService via HTTP  
**FR-002**: Support date range filtering (startDate, endDate) for trajectory queries  
**FR-003**: Support single-point position queries at specific timestamps  
**FR-004**: Handle CalculationService unavailability gracefully (circuit breaker)  
**FR-005**: Cache trajectory results to reduce load on CalculationService  
**FR-006**: Retry transient failures automatically (network timeouts, 5xx errors)  
**FR-007**: Log all HTTP interactions for observability  
**FR-008**: Support configurable timeouts (5s default, 30s max)

### Non-Functional Requirements

**NFR-001**: Response time < 2s for trajectory queries (P95)  
**NFR-002**: Handle 100 req/s sustained load  
**NFR-003**: 99.9% availability (exclude CalculationService downtime)  
**NFR-004**: Graceful degradation when cache unavailable  
**NFR-005**: Zero data loss during retries (idempotent requests)

---

## 🔌 API CONTRACT

### CalculationService Endpoints (Assumed)

```http
POST /api/calculations/trajectory
Content-Type: application/json

{
  "objectId": "2I/Borisov",
  "startDate": "2019-12-01T00:00:00Z",
  "endDate": "2019-12-31T23:59:59Z",
  "intervalHours": 6
}

Response 200 OK:
{
  "objectId": "2I/Borisov",
  "points": [
    {
      "timestamp": "2019-12-01T00:00:00Z",
      "position": { "x": 1.5, "y": 0.8, "z": 0.3 },
      "velocity": { "x": 0.02, "y": 0.01, "z": 0.005 }
    }
  ],
  "generatedAt": "2025-11-23T10:30:00Z"
}
```

```http
GET /api/calculations/position/{objectId}?date={iso8601}

Response 200 OK:
{
  "objectId": "2I/Borisov",
  "timestamp": "2019-12-08T00:00:00Z",
  "position": { "x": 7.249, "y": 0.0, "z": 0.0 },
  "velocity": { "x": 0.0, "y": 32.2, "z": 0.0 },
  "distanceFromSun": 7.249
}
```

### Error Responses

- `400 Bad Request`: Invalid objectId or date format
- `404 Not Found`: ObjectId not in database
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Calculation failure
- `503 Service Unavailable`: Service overloaded

---

## 🧪 TEST SCENARIOS (Pre-Implementation)

### Unit Tests (CalculationServiceClient)

#### Happy Path

1. ✅ `GetTrajectoryAsync_ValidRequest_ReturnsTrajectoryData`
2. ✅ `GetPositionAsync_ValidRequest_ReturnsPositionData`
3. ✅ `GetTrajectoryAsync_WithDateRange_SendsCorrectPayload`

#### Error Handling

4. ✅ `GetTrajectoryAsync_ObjectNotFound_ReturnsNull`
5. ✅ `GetTrajectoryAsync_ServiceUnavailable_ThrowsHttpRequestException`
6. ✅ `GetTrajectoryAsync_Timeout_ThrowsTimeoutException`
7. ✅ `GetTrajectoryAsync_InvalidResponse_ThrowsJsonException`

#### Retry Logic (Polly)

8. ✅ `GetTrajectoryAsync_TransientFailure_RetriesAndSucceeds`
9. ✅ `GetTrajectoryAsync_PermanentFailure_ThrowsAfterRetries`
10. ✅ `GetTrajectoryAsync_CircuitOpen_ThrowsBrokenCircuitException`

#### Caching

11. ✅ `GetTrajectoryAsync_CacheHit_DoesNotCallService`
12. ✅ `GetTrajectoryAsync_CacheMiss_CallsServiceAndCaches`
13. ✅ `GetTrajectoryAsync_CacheExpired_RefreshesData`

### Integration Tests (TrajectoryService with real HTTP)

14. ✅ `GetTrajectoryAsync_RealCalculationService_ReturnsValidData`
15. ✅ `GetTrajectoryAsync_ServiceDown_FallbackToCache`
16. ✅ `GetTrajectoryAsync_ConcurrentRequests_HandlesCorrectly`

---

## 📐 ARCHITECTURE DESIGN

### Components

```
TrajectoryService (existing)
    ↓ depends on
ICalculationServiceClient (NEW - interface)
    ↑ implemented by
CalculationServiceClient (NEW - HTTP client wrapper)
    ↓ uses
HttpClient (with Polly policies)
    ↓ calls
CalculationService (external microservice)
```

### Configuration

```json
{
  "CalculationService": {
    "BaseUrl": "http://localhost:5001",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 60
  },
  "Caching": {
    "TrajectoryTTLMinutes": 60,
    "Enabled": true
  }
}
```

---

## 📝 INPUT/OUTPUT SPECIFICATION

### ICalculationServiceClient Interface

```csharp
public interface ICalculationServiceClient
{
    /// <summary>
    /// Get trajectory data from CalculationService
    /// </summary>
    /// <param name="objectId">Interstellar object identifier</param>
    /// <param name="startDate">Start date for trajectory calculation</param>
    /// <param name="endDate">End date for trajectory calculation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trajectory points or null if object not found</returns>
    /// <exception cref="HttpRequestException">Service unavailable or network error</exception>
    /// <exception cref="TimeoutException">Request exceeded timeout</exception>
    Task<List<TrajectoryPoint>?> GetTrajectoryAsync(
        string objectId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position at specific time from CalculationService
    /// </summary>
    Task<(Vector3D Position, Vector3D Velocity, double Distance)?> GetPositionAsync(
        string objectId,
        DateTime date,
        CancellationToken cancellationToken = default);
}
```

### Known Data (2I/Borisov Reference)

- **Perihelion Date**: 2019-12-08T00:00:00Z
- **Perihelion Distance**: 2.006 AU (7.249472 AU in km)
- **Eccentricity**: 3.3569 (hyperbolic orbit)
- **Inclination**: 44.053°
- **Velocity at Perihelion**: ~32.2 km/s
- **Discovery**: 2019-08-30
- **Last Observation**: ~2020-03-20

---

## ✅ ACCEPTANCE CRITERIA

**AC-001**: All 16 tests written and failing (RED phase complete)  
**AC-002**: All 16 tests passing with minimal implementation (GREEN phase)  
**AC-003**: Code coverage ≥ 90% for new classes  
**AC-004**: No regression in existing 17 tests  
**AC-005**: Performance: <2s P95 for trajectory endpoint  
**AC-006**: Observability: All HTTP calls logged with duration  
**AC-007**: Configuration: Externalized in appsettings.json  
**AC-008**: Documentation: XML comments on all public APIs  

---

## 🚀 IMPLEMENTATION PHASES

### Phase 2: RED (Next Step)

- [ ] Create `ICalculationServiceClient.cs` interface
- [ ] Create `CalculationServiceClientTests.cs` with 13 unit tests
- [ ] Create `TrajectoryServiceIntegrationTests.cs` with 3 integration tests
- [ ] Verify ALL 16 tests FAIL (no implementation yet)
- [ ] Commit: "test: Add CalculationService integration tests (RED phase)"

### Phase 3: GREEN

- [ ] Create `CalculationServiceClient.cs` minimal implementation
- [ ] Configure Polly policies (retry, circuit breaker, timeout)
- [ ] Add in-memory caching with `IMemoryCache`
- [ ] Update `TrajectoryService` to use `ICalculationServiceClient`
- [ ] Verify ALL 16 new tests PASS + 17 existing tests PASS
- [ ] Commit: "feat: Implement CalculationService HTTP client (GREEN phase)"

### Phase 4: REFACTOR

- [ ] Extract Polly policies to separate configuration class
- [ ] Add structured logging with correlation IDs
- [ ] Optimize cache key strategy
- [ ] Add metrics/telemetry hooks
- [ ] Performance testing (100 req/s load test)
- [ ] Commit: "refactor: Improve CalculationService client quality"

### Phase 5: INTEGRATION

- [ ] Register services in `Program.cs` DI container
- [ ] Add `appsettings.json` configuration section
- [ ] Update `docker-compose.yml` with CalculationService mock
- [ ] Update documentation (README, architecture diagrams)
- [ ] End-to-end smoke test
- [ ] Commit: "chore: Complete CalculationService integration"

---

## 🎯 SUCCESS METRICS

- ✅ Zero production code written before tests
- ✅ Test coverage: 90%+ (target: 95%)
- ✅ All tests pass in <5s
- ✅ No manual testing required
- ✅ Documentation generated from code

**Estimated Effort**: 4-6 hours  
**Risk Level**: Medium (external dependency, network I/O)  
**Dependencies**: CalculationService API must be available (can mock with WireMock)

---

## 📚 References

- [Polly Documentation](https://github.com/App-vNext/Polly)
- [IHttpClientFactory Best Practices](https://learn.microsoft.com/aspnet/core/fundamentals/http-requests)
- [In-Memory Caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/memory)
- [Testing HTTP Clients with WireMock.Net](https://github.com/WireMock-Net/WireMock.Net)
