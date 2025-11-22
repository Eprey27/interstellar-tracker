# Event-Driven Transition Roadmap

## Overview

This document outlines the step-by-step evolution from the current hybrid monolith-microservices architecture to a fully event-driven microservices system using RabbitMQ and MassTransit.

## Current State (MVP - Phase 1)

```
┌─────────────────────────────────────────────┐
│           Shared Libraries                   │
│  ┌─────────────────────────────────────┐   │
│  │ Domain (Models, ValueObjects)       │   │
│  │ Application (MediatR, Use Cases)    │   │
│  │ Infrastructure (Repositories, DB)   │   │
│  └─────────────────────────────────────┘   │
└─────────────────┬───────────────────────────┘
                  │ (Referenced by all services)
        ┌─────────┼─────────────┐
        │         │             │
    ┌───▼───┐ ┌──▼──────┐ ┌───▼────┐
    │ Calc  │ │  WebUI  │ │ Admin  │
    │Service│ │ (Blazor)│ │ Service│
    └───┬───┘ └────┬────┘ └────────┘
        │          │
        │  HTTP    │
        └──────────┘
```

**Characteristics:**

- Synchronous HTTP/REST communication
- Shared domain/application/infrastructure libraries
- Single deployment unit per service
- Tight coupling via direct API calls
- Simple to develop and debug

## Target State (Event-Driven - Phase 4)

```
┌────────────────────────────────────────────────────────┐
│                  Message Contracts                      │
│           (InterstellarTracker.Messages)                │
│   Commands: CalculatePositionCommand, ...              │
│   Events: PositionCalculatedEvent, ...                 │
└─────────────────────┬──────────────────────────────────┘
                      │ (Shared by all services)
              ┌───────┴───────┐
              │   RabbitMQ    │
              │ Message Broker │
              └───────┬───────┘
                      │
        ┌─────────────┼─────────────┬─────────────┐
        │             │             │             │
    ┌───▼───┐   ┌────▼────┐   ┌───▼────┐   ┌────▼────┐
    │ Calc  │   │  WebUI  │   │ Cache  │   │  Audit  │
    │Service│   │         │   │Service │   │ Service │
    │       │   │         │   │        │   │         │
    │Own DB │   │         │   │ Redis  │   │ Own DB  │
    └───────┘   └─────────┘   └────────┘   └─────────┘
```

**Characteristics:**

- Asynchronous message-based communication
- Each service owns its data store
- Loose coupling via events and commands
- Independent scalability per service
- Saga pattern for distributed transactions
- Resilient to individual service failures

## Migration Phases

### Phase 1: HTTP-Only (CURRENT) ✅

**Status:** COMPLETED  
**Duration:** Sessions 1-6  

**Features:**

- Clean Architecture layers
- REST APIs with Swagger
- Blazor Server WebUI
- Health checks
- Prometheus/Grafana monitoring

**No Changes Required** - This is the MVP foundation.

---

### Phase 2: Hybrid (HTTP + Events) 📋

**Status:** PLANNED  
**Duration:** 2-3 weeks  
**Goal:** Introduce RabbitMQ without breaking existing functionality

#### Step 1: Infrastructure (2 days)

1. **Add RabbitMQ to docker-compose.yml**

   ```yaml
   rabbitmq:
     image: rabbitmq:3.13-management-alpine
     ports:
       - "5672:5672"   # AMQP
       - "15672:15672" # Management UI
     environment:
       RABBITMQ_DEFAULT_USER: interstellar
       RABBITMQ_DEFAULT_PASS: dev_password_123
     healthcheck:
       test: rabbitmq-diagnostics -q ping
   ```

2. **Install MassTransit NuGet packages**

   ```bash
   # All services
   dotnet add package MassTransit
   dotnet add package MassTransit.RabbitMQ
   dotnet add package MassTransit.Extensions.DependencyInjection
   ```

3. **Create Messages Library**

   ```bash
   dotnet new classlib -n InterstellarTracker.Messages
   dotnet sln add src/Messages/InterstellarTracker.Messages
   ```

#### Step 2: Message Contracts (2 days)

Create command and event DTOs in `InterstellarTracker.Messages`:

**Commands** (requests for action):

```csharp
// InterstellarTracker.Messages/Commands/CalculatePositionCommand.cs
public record CalculatePositionCommand
{
    public Guid CommandId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public string CelestialBodyId { get; init; } = string.Empty;
    public DateTimeOffset TargetDate { get; init; }
    public string RequestedBy { get; init; } = "anonymous";
}
```

**Events** (notifications of what happened):

```csharp
// InterstellarTracker.Messages/Events/PositionCalculatedEvent.cs
public record PositionCalculatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; }
    public string CelestialBodyId { get; init; } = string.Empty;
    public Vector3Dto Position { get; init; } = new();
    public Vector3Dto Velocity { get; init; } = new();
    public TimeSpan CalculationDuration { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
```

#### Step 3: Configure MassTransit (1 day)

**CalculationService/Program.cs:**

```csharp
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<CalculatePositionConsumer>();
    x.AddConsumer<UpdateOrbitConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("interstellar");
            h.Password("dev_password_123");
        });

        // Configure receive endpoints
        cfg.ReceiveEndpoint("calculation-service", e =>
        {
            e.ConfigureConsumer<CalculatePositionConsumer>(context);
        });

        // Prometheus metrics
        cfg.UsePrometheusMetrics(serviceName: "calculation_service");
    });
});
```

#### Step 4: Implement Consumers (3 days)

**CalculationService - Message Consumer:**

```csharp
public class CalculatePositionConsumer : IConsumer<CalculatePositionCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CalculatePositionConsumer> _logger;

    public async Task Consume(ConsumeContext<CalculatePositionCommand> context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Use existing MediatR handler
            var query = new GetCelestialBodyPositionQuery
            {
                CelestialBodyId = context.Message.CelestialBodyId,
                TargetDate = context.Message.TargetDate
            };

            var result = await _mediator.Send(query);

            // Publish event
            await context.Publish(new PositionCalculatedEvent
            {
                CorrelationId = context.Message.CorrelationId,
                CelestialBodyId = context.Message.CelestialBodyId,
                Position = new Vector3Dto(result.Position.X, result.Position.Y, result.Position.Z),
                Velocity = new Vector3Dto(result.Velocity.X, result.Velocity.Y, result.Velocity.Z),
                CalculationDuration = stopwatch.Elapsed
            });

            _logger.LogInformation(
                "Position calculated for {Body} via message bus in {Duration}ms",
                context.Message.CelestialBodyId, 
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate position for {Body}", 
                context.Message.CelestialBodyId);
            throw; // MassTransit will handle retry
        }
    }
}
```

**WebUI - Message Publisher:**

```csharp
// Services/CalculationServiceClient.cs (modified)
public class CalculationServiceClient
{
    private readonly IPublishEndpoint _publishEndpoint; // MassTransit
    private readonly HttpClient _httpClient; // Fallback

    public async Task<PositionDto> GetPositionAsync(string bodyId, DateTimeOffset date)
    {
        var correlationId = Guid.NewGuid();
        
        // Publish command to RabbitMQ
        await _publishEndpoint.Publish(new CalculatePositionCommand
        {
            CorrelationId = correlationId,
            CelestialBodyId = bodyId,
            TargetDate = date,
            RequestedBy = "webui"
        });

        // Option 1: Wait for response event (Request/Response pattern)
        // Option 2: Return immediately, update UI when event arrives (true async)
        // Option 3: Fallback to HTTP if message times out

        // For now, use HTTP as fallback while transitioning
        return await _httpClient.GetFromJsonAsync<PositionDto>(
            $"/api/celestialbodies/{bodyId}/position?date={date:O}");
    }
}
```

#### Step 5: Create First Pure Event Service (2 days)

**InterstellarTracker.CacheService** - new project:

```csharp
public class PositionCacheConsumer : IConsumer<PositionCalculatedEvent>
{
    private readonly IDistributedCache _cache; // Redis

    public async Task Consume(ConsumeContext<PositionCalculatedEvent> context)
    {
        var cacheKey = $"position:{context.Message.CelestialBodyId}:{context.Message.TargetDate:O}";
        
        var cacheEntry = JsonSerializer.Serialize(new
        {
            context.Message.Position,
            context.Message.Velocity,
            CachedAt = DateTimeOffset.UtcNow
        });

        await _cache.SetStringAsync(cacheKey, cacheEntry, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });

        // Publish cache updated event
        await context.Publish(new PositionCachedEvent
        {
            CelestialBodyId = context.Message.CelestialBodyId,
            CacheKey = cacheKey
        });
    }
}
```

#### Testing Phase 2

- [ ] Unit tests with `InMemoryTestHarness`
- [ ] Integration tests: publish command, verify event published
- [ ] Load test: 1000 requests/sec via RabbitMQ
- [ ] Verify HTTP still works (fallback)
- [ ] RabbitMQ Management UI shows message flow

---

### Phase 3: Event-First (HTTP becomes facade) 📋

**Status:** FUTURE  
**Duration:** 3-4 weeks  
**Goal:** New features use events, HTTP endpoints become thin wrappers

#### Changes

1. **HTTP Controllers become Publishers**

   ```csharp
   [HttpGet("{id}/position")]
   public async Task<IActionResult> GetPosition(string id, [FromQuery] DateTimeOffset date)
   {
       // Just publish command, don't wait
       await _publishEndpoint.Publish(new CalculatePositionCommand
       {
           CelestialBodyId = id,
           TargetDate = date
       });

       return Accepted(); // 202 Accepted, result will arrive via event
   }
   ```

2. **Introduce Saga Pattern**
   - `InterstellarObjectDiscoverySaga`
   - Multi-step workflows with compensation
   - State persistence with EF Core

3. **CQRS Separation**
   - Commands go through message bus
   - Queries read from materialized views (cache)
   - Eventually consistent

---

### Phase 4: Pure Microservices 📋

**Status:** FUTURE  
**Duration:** 6-8 weeks  
**Goal:** Complete autonomy, each service owns data

#### Changes

1. **Database per Service**
   - CalculationService → PostgreSQL (calculations)
   - CacheService → Redis (hot data)
   - AuditService → PostgreSQL (events)
   - NotificationService → PostgreSQL (subscriptions)

2. **Remove HTTP Between Services**
   - All communication via RabbitMQ
   - API Gateway only for external clients

3. **Event Sourcing (Optional)**
   - Store events as source of truth
   - Rebuild state from event log
   - Time travel debugging

## Decision Points

### When to Move to Next Phase?

**Phase 1 → Phase 2 (Add RabbitMQ):**

- ✅ MVP is deployed to Azure
- ✅ Application Insights integrated
- ✅ Authentication working
- ✅ Team comfortable with current architecture

**Phase 2 → Phase 3 (Event-First):**

- ✅ RabbitMQ proven stable in production
- ✅ Message patterns well understood
- ✅ Observability (tracing) in place
- ✅ At least 2 services using events successfully

**Phase 3 → Phase 4 (Pure Microservices):**

- ✅ Need for independent scaling demonstrated
- ✅ Team has experience with distributed systems
- ✅ Saga pattern proven with real workflows
- ✅ Business justification for added complexity

## Rollback Strategy

At each phase, maintain ability to rollback:

**Phase 2:** RabbitMQ fails → HTTP still works  
**Phase 3:** Event issues → Can fall back to HTTP  
**Phase 4:** Database separation problems → Can revert to shared DB

## Metrics for Success

### Phase 2 Success Criteria

- [ ] RabbitMQ uptime > 99.9%
- [ ] Message latency < 50ms p99
- [ ] Zero message loss
- [ ] HTTP and events coexist without conflicts
- [ ] Team confident publishing/consuming messages

### Phase 3 Success Criteria

- [ ] >50% of requests use event path
- [ ] Saga workflow completion rate > 99%
- [ ] Distributed tracing shows full message flow
- [ ] No cascading failures from one service

### Phase 4 Success Criteria

- [ ] Each service independently deployable
- [ ] Service failures don't affect others
- [ ] Can scale services based on load
- [ ] Event replay works for debugging

## References

- **ADR-004:** `docs/adr/004-event-driven-microservices.md`
- **MassTransit Docs:** <https://masstransit.io/>
- **RabbitMQ Tutorial:** <https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html>
- **Saga Pattern:** <https://microservices.io/patterns/data/saga.html>

## Next Actions

Current focus: **Complete Phase 1 MVP**

- Application Insights
- Azure deployment
- Authentication

After MVP: **Begin Phase 2** (estimated Q1 2026)
