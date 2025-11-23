# System Overview

## Table of Contents

- [Architecture Style](#architecture-style)
- [High-Level Architecture](#high-level-architecture)
- [Microservices](#microservices)
- [Communication Patterns](#communication-patterns)
- [Technology Stack](#technology-stack)
- [Deployment Model](#deployment-model)

## Architecture Style

**Clean Architecture + Microservices + Event-Driven**

Seguimos los principios de Clean Architecture (Uncle Bob) aplicados a una arquitectura de microservicios. Cada servicio mantiene separación de capas (Domain, Application, Infrastructure) y se comunica via eventos asíncronos.

### Architectural Drivers

- **Maintainability**: Testable, código desacoplado
- **Scalability**: Servicios independientes escalables
- **Observability**: Trazabilidad end-to-end
- **Performance**: Rendering 3D 60fps, cálculos <100ms

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Users/Clients                         │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
            ┌────────────────────────┐
            │   API Gateway (YARP)   │ :5014
            │  + Load Balancing      │
            └───────────┬────────────┘
                        │
        ┌───────────────┼───────────────┐
        ▼               ▼               ▼
┌───────────────┐ ┌──────────────┐ ┌─────────────┐
│ CalculationSvc│ │ VisualizSvc  │ │  AuthService│
│    :5001      │ │    :5002     │ │   :5003     │
└───────┬───────┘ └──────┬───────┘ └──────┬──────┘
        │                │                 │
        └────────────────┼─────────────────┘
                         ▼
                 ┌───────────────┐
                 │   RabbitMQ    │
                 │ Event Bus     │
                 └───────────────┘
                         │
                         ▼
            ┌────────────────────────┐
            │  Azure App Insights    │
            │  + Log Analytics       │
            └────────────────────────┘
```

## Microservices

### 1. API Gateway (:5014)

- **Tech**: YARP (Yet Another Reverse Proxy)
- **Purpose**: Routing, load balancing, rate limiting
- **Status**: ✅ 60% complete

### 2. CalculationService (:5001)

- **Purpose**: Cálculos orbitales (posición, velocidad, trayectoria)
- **Domain**: Hyperbolic orbits, coordinate transformations
- **Status**: ✅ 70% complete

### 3. VisualizationService (:5002)

- **Purpose**: Endpoints para datos de rendering
- **Features**: WebSockets, coordinate transforms
- **Status**: ⏳ 0% (Phase 2)

### 4. AuthService (:5003)

- **Purpose**: Autenticación/autorización (Keycloak integration)
- **Status**: ⏳ 0% (Phase 3)

### 5. WebUI (Blazor)

- **Purpose**: Dashboard, management interface
- **Tech**: Blazor Server + SignalR
- **Status**: ⏳ 20% (skeleton)

### 6. 3D Rendering (Desktop)

- **Purpose**: Visualización OpenGL 3D interactiva
- **Tech**: Silk.NET + OpenGL
- **Status**: ⏳ 0% (Phase 2)

## Communication Patterns

### Synchronous (Current)

- HTTP/REST entre servicios
- YARP para routing

### Asynchronous (Phase 4)

- RabbitMQ message broker
- Event-driven: `OrbitCalculated`, `PositionUpdated`, `VisualizationRequested`
- Pub/Sub + Request/Reply patterns

## Technology Stack

### Backend

- **.NET 8.0** (target .NET 9 Q1 2026)
- **ASP.NET Core** (Web APIs)
- **YARP** (API Gateway)
- **xUnit** (Testing)
- **FluentValidation** (Validation)
- **MediatR** (CQRS patterns)

### Frontend

- **Blazor Server** (WebUI)
- **Silk.NET** (OpenGL rendering)
- **SignalR** (Real-time updates)

### Infrastructure

- **Azure App Insights** (Monitoring)
- **Azure Key Vault** (Secrets)
- **RabbitMQ** (Message broker - Phase 4)
- **Keycloak** (Auth - Phase 3)
- **Docker** (Containerization)
- **Kubernetes** (Orchestration)
- **Terraform** (IaC)

### Development

- **SonarQube** (Code quality - Phase 5)
- **Coverlet** (Code coverage)
- **Docker Compose** (Local dev)
- **Prometheus + Grafana** (Observability)

## Deployment Model

### Local Development

```
docker-compose up -d
dotnet run --project src/Services/CalculationService
```

### Azure Development (Current)

- Resource Group: `rg-interstellar-tracker-dev`
- Application Insights: `interstellar-tracker-dev-ai`
- Key Vault: `interstellar-dev-kv`
- Log Analytics: `interstellar-tracker-dev-law`

### Production (Planned)

- AKS (Azure Kubernetes Service)
- Multi-region deployment
- Auto-scaling policies
- Blue/Green deployments
