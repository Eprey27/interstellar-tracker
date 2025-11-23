# 🗺️ Roadmap - Interstellar Tracker

> **Principio de Completion Culture**: *"Termina lo que empieces o no terminarás nunca nada"*  
> Este roadmap guía el **100% completado de InterstellarTracker** antes de iniciar nuevos proyectos.

## 📅 Estado Actual (2025-11-23)

### ✅ Completado

- [x] Arquitectura limpia con microservicios (.NET 9)
- [x] Domain models y cálculos orbitales hiperbólicos
- [x] CalculationService con API REST
- [x] ApiGateway con YARP (reverse proxy)
- [x] Infrastructure as Code (Terraform profesional)
- [x] Azure Application Insights desplegado (10 recursos)
- [x] Monitoreo: Prometheus + Grafana + App Insights
- [x] Docker Compose configurado
- [x] Health checks en todos los servicios
- [x] User secrets para desarrollo local
- [x] Tests unitarios básicos (xUnit)

### 🎯 Servicios Activos

- **ApiGateway**: <http://localhost:5014> (YARP, health checks, telemetría)
- **CalculationService**: <http://localhost:5001> (cálculos orbitales)
- **Azure Portal**: Application Insights con alertas configuradas

---

## 🚀 Próxima Sesión - Plan de Acción

### 1️⃣ Documentación Completa del Proyecto (Prioridad ALTA)

**Objetivo**: Documentar exhaustivamente toda la arquitectura y código existente

#### Tareas

- [ ] **README.md principal**: Actualizar con arquitectura actual, servicios, guías de inicio
- [ ] **Documentación técnica**:
  - [ ] `docs/architecture/system-overview.md` - Diagrama de arquitectura general
  - [ ] `docs/architecture/microservices.md` - Detalle de cada microservicio
  - [ ] `docs/architecture/data-flow.md` - Flujo de datos entre servicios
  - [ ] `docs/development/getting-started.md` - Guía para nuevos desarrolladores
  - [ ] `docs/development/local-setup.md` - Configuración local paso a paso
  - [ ] `docs/deployment/azure-infrastructure.md` - Infraestructura Azure desplegada
- [ ] **XML Documentation**: Revisar que todos los métodos públicos tengan comentarios XML
- [ ] **Swagger/OpenAPI**: Mejorar descripciones de endpoints
- [ ] **ADR (Architecture Decision Records)**: Actualizar decisiones recientes
  - [ ] ADR-005: Elección de YARP para API Gateway
  - [ ] ADR-006: Application Insights vs alternativas
  - [ ] ADR-007: Mensajería (RabbitMQ vs Kafka)

**Entregables**:

- Documentación completa en `docs/`
- README actualizado con badges de build/coverage
- Diagramas C4 o similares

---

### 2️⃣ VisualizationService (Prioridad ALTA)

**Objetivo**: Crear servicio para procesar datos orbitales para renderizado 3D

#### Funcionalidades

- [ ] **API REST** con endpoints:
  - `GET /api/trajectories/{objectId}` - Trayectoria completa
  - `GET /api/positions/{objectId}?date={date}` - Posición en fecha específica
  - `GET /api/orbital-elements/{objectId}` - Elementos orbitales para visualización
  - `POST /api/coordinate-transform` - Transformar coordenadas heliocéntricas → 3D
- [ ] **Caché de trayectorias**: Redis o in-memory cache
- [ ] **WebSocket support**: Updates en tiempo real para posiciones
- [ ] **Optimizaciones 3D**:
  - Reducción de puntos (Douglas-Peucker algorithm)
  - LOD (Level of Detail) según zoom
  - Bounding boxes para culling
- [ ] **Integration con CalculationService**: Consumir datos orbitales
- [ ] **Health checks y telemetría**: Application Insights
- [ ] **Tests unitarios** (TDD desde el inicio)

**Puerto**: <http://localhost:5002>

**Entregables**:

- Proyecto `InterstellarTracker.VisualizationService` funcionando
- Tests con >80% cobertura
- Documentación de API (Swagger)
- Integrado en ApiGateway

---

### 3️⃣ Descomposición en Microservicios (Prioridad MEDIA)

**Objetivo**: Desgranar servicios actuales en microservicios especializados

#### Propuesta de Microservicios

**Dominio: Orbital Calculations**

- [ ] **OrbitalCalculationService** (puerto 5011)
  - Cálculos de posición orbital
  - Ephemerides
  - Predicciones futuras
  
- [ ] **OrbitalElementsService** (puerto 5012)
  - Gestión de elementos orbitales
  - Validación de parámetros
  - Conversiones entre sistemas

**Dominio: Visualization**

- [ ] **TrajectoryService** (puerto 5021)
  - Generación de trayectorias optimizadas
  - Caché de paths calculados
  
- [ ] **CoordinateTransformService** (puerto 5022)
  - Transformaciones de coordenadas
  - Sistemas de referencia

**Dominio: Data Management**

- [ ] **ObjectCatalogService** (puerto 5031)
  - CRUD de objetos interestelares
  - Metadatos y clasificación
  
- [ ] **ObservationService** (puerto 5032)
  - Datos observacionales
  - Integración con telescopios/APIs externas

**Cross-cutting Concerns**

- [ ] **EventBusService**: Mensajería centralizada
- [ ] **ConfigurationService**: Configuración centralizada
- [ ] **LoggingService**: Agregación de logs

**Entregables**:

- Nuevos proyectos de microservicios
- ApiGateway actualizado con todas las rutas
- Docker Compose con todos los servicios
- Documentación de cada microservicio

---

### 4️⃣ Event-Driven Architecture con Mensajería (Prioridad ALTA)

**Objetivo**: Implementar comunicación asíncrona entre microservicios

#### Decisión: RabbitMQ vs Apache Kafka

**Análisis**:

| Criterio | RabbitMQ | Apache Kafka |
|----------|----------|--------------|
| Complejidad | Baja | Media-Alta |
| Latencia | Muy baja (~1ms) | Baja (~5-10ms) |
| Throughput | Alto | Muy Alto |
| Event Sourcing | Limitado | Excelente |
| Persistencia | Temporal | Permanente (log) |
| Operación | Más simple | Requiere Zookeeper/KRaft |
| Caso de uso | Request/Reply, RPC | Event streaming, logs |

**Recomendación Inicial**: **RabbitMQ**

- Más simple para empezar
- Suficiente para el volumen actual
- Mejor para request/reply patterns
- Fácil migración a Kafka si es necesario

#### Implementación RabbitMQ

**Exchanges y Queues**:

```
Exchange: orbital.calculations (topic)
  └─ Queue: orbital.position.requests
  └─ Queue: orbital.ephemeris.requests

Exchange: visualization.events (topic)
  └─ Queue: trajectory.cache.invalidation
  └─ Queue: coordinate.transform.requests

Exchange: catalog.events (fanout)
  └─ Queue: object.created
  └─ Queue: object.updated
  └─ Queue: object.deleted
```

**Tareas**:

- [ ] **Contenedor RabbitMQ**: Agregar a docker-compose.yml
- [ ] **Management UI**: <http://localhost:15672>
- [ ] **MassTransit o RawRabbit**: Librería para .NET
- [ ] **Event contracts**: Definir eventos en `Domain/Events/`
- [ ] **Publishers**: En cada microservicio
- [ ] **Consumers**: Handlers para eventos
- [ ] **Dead Letter Queues**: Para mensajes fallidos
- [ ] **Retry policies**: Exponential backoff
- [ ] **Monitoring**: Health checks RabbitMQ
- [ ] **Tests de integración**: Con TestContainers

**Eventos ejemplo**:

```csharp
// Domain/Events/OrbitalPositionCalculated.cs
public record OrbitalPositionCalculated(
    int ObjectId,
    DateTimeOffset Timestamp,
    Vector3 Position,
    Vector3 Velocity
);

// Domain/Events/ObjectCatalogUpdated.cs
public record ObjectCatalogUpdated(
    int ObjectId,
    string Name,
    OrbitalElements Elements
);
```

**Entregables**:

- RabbitMQ funcionando en Docker
- 3-5 eventos implementados
- Comunicación asíncrona entre servicios
- Monitoring de mensajería
- Tests de integración

---

### 5️⃣ Quality Gate con SonarQube (Prioridad ALTA)

**Objetivo**: Análisis estático de código con métricas de calidad

#### Configuración SonarQube

**Contenedor Docker**:

```yaml
# docker-compose.yml
sonarqube:
  image: sonarqube:10-community
  ports:
    - "9000:9000"
  environment:
    - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true
  volumes:
    - sonarqube_data:/opt/sonarqube/data
    - sonarqube_logs:/opt/sonarqube/logs
    - sonarqube_extensions:/opt/sonarqube/extensions
```

**Quality Gates configurados**:

- [ ] **Cobertura de código**: Mínimo 80%
- [ ] **Code Smells**: Rating A (0 smells críticos)
- [ ] **Bugs**: 0 bugs críticos/bloqueantes
- [ ] **Vulnerabilidades**: 0 críticas
- [ ] **Security Hotspots**: Revisados al 100%
- [ ] **Duplicación**: Máximo 3%
- [ ] **Complejidad ciclomática**: Máximo 15 por método
- [ ] **Deuda técnica**: <5% del tiempo de desarrollo

**Integración con CI/CD**:

- [ ] GitHub Actions workflow para análisis
- [ ] Quality Gate check antes de merge
- [ ] Reportes en Pull Requests

**Tareas**:

- [ ] Levantar SonarQube en Docker
- [ ] Crear proyecto en SonarQube
- [ ] Configurar `sonar-project.properties`
- [ ] Ejecutar primer análisis: `dotnet sonarscanner begin/end`
- [ ] Configurar Quality Gates personalizados
- [ ] Fix issues críticos existentes
- [ ] Documentar proceso en `docs/quality/sonarqube.md`

**Entregables**:

- SonarQube funcionando en <http://localhost:9000>
- Quality Gates configurados
- 0 issues críticos/bloqueantes
- Rating A en mantenibilidad

---

### 6️⃣ Test-Driven Development (TDD) - Nuevo Enfoque (Prioridad CRÍTICA)

**Objetivo**: Adoptar TDD como metodología principal + tests como documentación

#### Principios TDD

1. **Red**: Escribir test que falle
2. **Green**: Implementar código mínimo para pasar
3. **Refactor**: Mejorar sin romper tests

#### Estrategia de Testing

**Estructura de tests**:

```
tests/
├── Unit.Tests/                    # Tests unitarios (80% cobertura)
│   ├── Domain.Tests/
│   │   ├── Entities/
│   │   │   └── InterstellarObjectTests.cs  # Documenta comportamiento del objeto
│   │   ├── ValueObjects/
│   │   │   └── OrbitalElementsTests.cs     # Documenta validaciones
│   │   └── Services/
│   │       └── OrbitalCalculatorTests.cs   # Documenta cálculos
│   ├── Application.Tests/
│   │   ├── Commands/
│   │   │   └── CalculatePositionCommandTests.cs
│   │   └── Queries/
│   │       └── GetObjectQueryTests.cs
│   └── Services.Tests/
│       ├── CalculationService.Tests/
│       │   └── Controllers/
│       │       └── InterstellarObjectsControllerTests.cs
│       └── VisualizationService.Tests/
│
├── Integration.Tests/             # Tests de integración (50% cobertura)
│   ├── API.Tests/
│   │   └── CalculationServiceIntegrationTests.cs  # WebApplicationFactory
│   ├── MessageBroker.Tests/
│   │   └── RabbitMQIntegrationTests.cs           # TestContainers
│   └── Database.Tests/
│       └── RepositoryIntegrationTests.cs         # Test database
│
├── E2E.Tests/                     # Tests end-to-end (20% cobertura)
│   └── Scenarios/
│       └── CalculateTrajectoryE2ETests.cs       # Docker Compose completo
│
└── Performance.Tests/             # Load testing
    └── LoadTests/
        └── CalculationServiceLoadTests.cs       # NBomber o K6
```

**Tests como Documentación**:

```csharp
/// <summary>
/// Documenta el comportamiento del cálculo de posición orbital para órbitas hiperbólicas.
/// Casos de prueba basados en 2I/Borisov (C/2019 Q4).
/// </summary>
[TestClass]
public class HyperbolicOrbitCalculatorTests
{
    [TestMethod]
    [TestCategory("Documentation")]
    [TestCategory("OrbitalMechanics")]
    public void CalculatePosition_WithBorisovElements_ReturnsExpectedPositionAtPerihelion()
    {
        // Arrange: Elementos orbitales de Borisov en perihelio
        var elements = new OrbitalElements(
            eccentricity: 3.3571,
            semiMajorAxis: -0.8515, // AU (negativo para hiperbólica)
            inclination: 44.053,    // grados
            longitudeOfAscendingNode: 308.152,
            argumentOfPeriapsis: 209.124,
            meanAnomalyAtEpoch: 0.0 // Perihelio
        );
        
        var calculator = new HyperbolicOrbitCalculator();
        
        // Act: Calcular posición en perihelio (2019-12-08)
        var position = calculator.CalculatePosition(
            elements, 
            new DateTimeOffset(2019, 12, 8, 0, 0, 0, TimeSpan.Zero)
        );
        
        // Assert: Distancia perihelio esperada = q = a(1-e) = 2.006 AU
        var distance = position.Magnitude;
        Assert.AreEqual(2.006, distance, delta: 0.01, 
            "Distancia en perihelio debe ser 2.006 AU ± 0.01");
        
        // Assert: Posición en el plano eclíptico esperado
        Assert.IsTrue(Math.Abs(position.Z) < 0.5, 
            "Coordenada Z pequeña cerca del perihelio");
    }
    
    [TestMethod]
    public void CalculateVelocity_AtPerihelion_ExceedsEscapeVelocity()
    {
        // Este test documenta que las órbitas hiperbólicas superan velocidad de escape
        // ...
    }
}
```

**Cobertura Objetivo**:

- **Unit Tests**: 80%+ (crítico)
- **Integration Tests**: 60%+
- **E2E Tests**: Escenarios principales
- **Performance Tests**: Endpoints críticos

**Herramientas**:

- [ ] **xUnit**: Framework principal
- [ ] **FluentAssertions**: Asserts legibles
- [ ] **Moq**: Mocking
- [ ] **AutoFixture**: Datos de prueba
- [ ] **Bogus**: Datos fake realistas
- [ ] **TestContainers**: Integración con Docker
- [ ] **Coverlet**: Cobertura de código
- [ ] **ReportGenerator**: Reportes HTML
- [ ] **NBomber o K6**: Performance testing

**Tareas**:

- [ ] Refactorizar tests existentes con documentación mejorada
- [ ] Implementar tests faltantes para llegar a 80%
- [ ] Crear `docs/testing/tdd-guidelines.md`
- [ ] Setup CI/CD con ejecución de tests automática
- [ ] Generar reportes de cobertura en cada PR
- [ ] **Regla**: No merge sin tests + >80% cobertura

**Entregables**:

- Tests unitarios documentando cada caso de uso
- Cobertura >80% en SonarQube
- Pipeline CI/CD con quality gates
- Guía TDD para el equipo

---

### 7️⃣ Revisión General y Limpieza (Prioridad MEDIA)

**Objetivo**: Consolidar todo el trabajo previo

#### Checklist de Revisión

**Código**:

- [ ] Todos los servicios compilan sin warnings
- [ ] No hay código comentado sin motivo
- [ ] Nomenclatura consistente en todo el proyecto
- [ ] Principios SOLID aplicados
- [ ] DRY: No hay duplicación significativa

**Tests**:

- [ ] Todos los tests pasan
- [ ] Cobertura >80% en componentes críticos
- [ ] Tests son legibles y documentan comportamiento
- [ ] No hay tests ignorados sin justificación

**Documentación**:

- [ ] README actualizado con estado real
- [ ] Todos los ADR documentados
- [ ] Guías de desarrollo completas
- [ ] API documentation actualizada (Swagger)
- [ ] Diagramas reflejan arquitectura actual

**Infraestructura**:

- [ ] Docker Compose funciona en limpio
- [ ] Scripts de setup probados
- [ ] Variables de entorno documentadas
- [ ] Secrets no commiteados

**Calidad**:

- [ ] SonarQube rating A
- [ ] 0 security vulnerabilities
- [ ] Deuda técnica <5%
- [ ] Performance dentro de SLAs

---

## 📊 Métricas de Éxito

Al finalizar todas las tareas, el proyecto debe cumplir:

✅ **Arquitectura**:

- 6-8 microservicios independientes y desplegables
- Event-driven con RabbitMQ
- API Gateway centralizando acceso

✅ **Calidad**:

- Cobertura de tests >80%
- SonarQube rating A
- 0 bugs/vulnerabilities críticas

✅ **Documentación**:

- README completo y actualizado
- Docs técnicos exhaustivos
- Tests documentando comportamiento

✅ **DevOps**:

- CI/CD con quality gates
- Docker Compose con todos los servicios
- Monitoreo completo (App Insights + Prometheus)

✅ **TDD**:

- Todos los nuevos features con tests primero
- Tests como documentación viva
- Pipeline bloqueando sin cobertura

---

## 🎯 Orden de Ejecución Recomendado

**Fase 1 - Fundamentos (Sesión 1)**:

1. Documentación actual (README, arquitectura)
2. TDD setup (guidelines, tools)
3. SonarQube container + primer análisis

**Fase 2 - Nuevos Servicios (Sesión 2-3)**:
4. VisualizationService (TDD desde inicio)
5. RabbitMQ integration
6. Descomposición en microservicios

**Fase 3 - Consolidación (Sesión 4)**:
7. Tests faltantes para 80% cobertura
8. Fix issues críticos SonarQube
9. Documentación completa

**Fase 4 - Refinamiento (Sesión 5)**:
10. Revisión general y limpieza
11. Performance testing
12. Preparación para producción

---

## 📝 Notas para la Próxima Sesión

**Contexto Importante**:

- Terraform state en Azure Storage (backend configurado)
- Application Insights activo con 10 recursos en West Europe
- Alertas configuradas (high-failure-rate ya probada)
- User secrets configurados en CalculationService, WebUI, ApiGateway
- Puertos ocupados: 5001 (Calc), 5014 (Gateway), 5159 (WebUI)

**Quick Start Commands**:

```powershell
# Iniciar servicios actuales
cd d:\Repos\astronomy\interstellar-tracker
docker-compose up -d                                    # Prometheus, Grafana, PostgreSQL
dotnet run --project src/Services/CalculationService/...
dotnet run --project src/Services/ApiGateway/...

# Ver estado
curl http://localhost:5014/health
curl http://localhost:5001/health

# Terraform
cd terraform/environments/dev
terraform plan
terraform apply

# Tests
dotnet test
dotnet test /p:CollectCoverage=true
```

**Prioridad Absoluta**: TDD + Documentación + SonarQube

---

## 🔮 Future Vision: Project Intelligence Agent (PIA)

**Status**: CONCEPT (Post-InterstellarTracker)  
**Document**: [`PIA-VISION.md`](./PIA-VISION.md)

### Concepto

Multi-agent LLM system para desarrollo de software con:

- **Onboarding instantáneo**: 0 → productivo en <1 hora
- **Compliance as Code**: GDPR, ISO27001, IEC62443 automation
- **Multi-stakeholder**: Traduce contexto técnico para cualquier audiencia
- **Living Documentation**: Docs auto-generados desde codebase
- **Specialist Agents**: Architecture Expert, Code Mentor, Testing Specialist, Security Auditor, Compliance Officer, Communications Translator

### Strategic Decision

**NO implementar hasta completar InterstellarTracker 100%**. PIA se desarrollará como proyecto independiente cuando:

1. ✅ InterstellarTracker completado (todas fases 1-7)
2. ✅ Principio de completion culture validado
3. ✅ InterstellarTracker como primer use case de PIA

**Rationale**: *"Un proyecto terminado al 80% vale infinitamente más que 10 proyectos al 50%"*

**Next Review**: Post-InterstellarTracker completion (Q2 2026)

Ver [`PIA-VISION.md`](./PIA-VISION.md) para concepto completo (arquitectura, business model, roadmap, competencia).

---

**Creado**: 2025-11-23  
**Última Actualización**: 2025-11-23 (PIA Future Vision añadida)
**Última actualización**: 2025-11-23  
**Estado**: 🟢 Listo para siguiente sesión
