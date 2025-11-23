# 🌌 Interstellar Tracker

> A professional 3D visualization system for tracking the passage of interstellar object **2I/Borisov** through our solar system.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/tests-23%20passing-brightgreen.svg)]()
[![Coverage](https://img.shields.io/badge/coverage-85%25-brightgreen.svg)]()
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Azure](https://img.shields.io/badge/azure-not%20deployed-lightgrey?logo=microsoft-azure)](https://portal.azure.com/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

## 🎯 Project Overview

**Interstellar Tracker** is a microservices-based application that provides real-time and time-accelerated visualization of interstellar objects passing through our solar system. Built with Clean Architecture principles, it demonstrates professional .NET development practices suitable for learning and production use.

### Key Features

- 🌠 **3D Solar System Visualization** - OpenGL-based rendering using Silk.NET
- ⏱️ **Time Control** - Real-time and accelerated time simulation
- 🔐 **Enterprise Authentication** - Keycloak integration with social login support
- 📊 **Orbital Calculations** - Accurate astronomical physics engine
- 🐳 **Container-Ready** - Full Docker and Kubernetes support
- ✅ **Quality Assured** - Comprehensive testing and code quality gates
- 📚 **Well-Documented** - Designed for junior developer onboarding

## 🏗️ Architecture

This project follows **Clean Architecture** with microservices pattern and event-driven communication:

```text
┌─────────────────────────────────────────────────────────────┐
│                    API Gateway (YARP)                        │
│           Routing • Load Balancing • Telemetry              │
└────────────────────────┬────────────────────────────────────┘
                         │
        ┌────────────────┼────────────────┐
        ▼                ▼                ▼
┌───────────────┐ ┌──────────────┐ ┌─────────────┐
│ CalculationSvc│ │VisualizSvc   │ │ AuthService │
│    :5001      │ │   :5002      │ │   :5003     │
│ • Orbital calc│ │• Trajectories│ │• Keycloak   │
│ • Ephemeris   │ │• Coord. trans│ │• JWT        │
└───────┬───────┘ └──────┬───────┘ └──────┬──────┘
        │                │                 │
        └────────────────┼─────────────────┘
                         ▼
                 ┌───────────────┐
                 │   RabbitMQ    │ (Phase 4)
                 │  Event Bus    │
                 └───────┬───────┘
                         │
            ┌────────────┴────────────┐
            ▼                         ▼
    ┌────────────────┐       ┌────────────────┐
    │ App Insights   │       │   Grafana +    │
    │ + Log Analytics│       │   Prometheus   │
    └────────────────┘       └────────────────┘
```

**Current Status**: ✅ API Gateway + CalculationService deployed | ✅ VisualizationService HTTP integration (TDD)

### Microservices

| Service | Port | Status | Description |
|---------|------|--------|-------------|
| **API Gateway** | 5014 | ✅ 60% | YARP reverse proxy, routing, telemetry |
| **CalculationService** | 5001 | ✅ 70% | Orbital calculations, hyperbolic orbits |
| **VisualizationService** | 5002 | ✅ 35% | Trajectory data, coordinate transforms, **HTTP client to CalculationService** |
| **AuthService** | 5003 | ⏳ 0% | Keycloak integration, JWT validation |
| **WebUI (Blazor)** | 5015 | ⏳ 20% | Dashboard, management interface |
| **3D Rendering** | - | ⏳ 0% | Silk.NET + OpenGL desktop client |

### Layer Structure

- **Domain** - Core business models (HyperbolicOrbit, CelestialBody, 2I/Borisov)
- **Application** - Use cases, CQRS (MediatR), validation (FluentValidation)
- **Infrastructure** - Azure App Insights, Key Vault, persistence
- **Services** - Microservices comunicating via HTTP (→ RabbitMQ Phase 4)
- **Web** - Blazor Server dashboard + Silk.NET 3D rendering

📚 **Detailed Architecture**: [`docs/02-architecture/system-overview.md`](docs/02-architecture/system-overview.md)

## 🚀 Quick Start

**Target**: 0 → Running system in < 30 minutes

### Prerequisites

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download)** (or later)
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (for infrastructure)
- **[Git](https://git-scm.com/)**
- **VS Code** (recommended) or Visual Studio 2022+

### 5-Minute Setup

```powershell
# 1. Clone and navigate
git clone https://github.com/YOUR_USERNAME/interstellar-tracker.git
cd interstellar-tracker

# 2. Start infrastructure (Keycloak, RabbitMQ, Prometheus, Grafana)
docker-compose up -d

# 3. Build solution
dotnet build InterstellarTracker.sln

# 4. Run tests
dotnet test

# 5. Start services (VS Code task or manual)
dotnet run --project src/Services/CalculationService/InterstellarTracker.CalculationService
dotnet run --project src/Services/ApiGateway/InterstellarTracker.ApiGateway
```

### Verify Services

- **API Gateway**: <http://localhost:5014/swagger>
- **CalculationService**: <http://localhost:5001/swagger>
- **Health checks**: <http://localhost:5014/health>
- **Grafana**: <http://localhost:3000> (admin/admin)
- **RabbitMQ Management**: <http://localhost:15672> (guest/guest)

📘 **Complete Guide**: [`docs/04-development/getting-started.md`](docs/04-development/getting-started.md)

## 📖 Documentation

### 🏗️ Architecture

- [**System Overview**](docs/02-architecture/system-overview.md) - High-level architecture, tech stack, deployment
- [**Microservices**](docs/02-architecture/microservices.md) - Service catalog, dependencies, API contracts
- [**Data Flow**](docs/02-architecture/data-flow.md) - Request/event flow, caching strategy
- [**ADRs**](docs/03-adr/) - Architecture Decision Records
  - [ADR-001: Clean Architecture + Microservices](docs/03-adr/001-clean-architecture-microservices.md)
  - [ADR-005: YARP for API Gateway](docs/03-adr/005-yarp-api-gateway.md)
  - [ADR-006: Application Insights](docs/03-adr/006-application-insights.md)
  - [ADR-007: CalculationService Integration (TDD)](docs/03-adr/007-calculation-service-integration-tdd.md) ⭐ **NEW**

### 👨‍💻 Development

- [**Getting Started**](docs/04-development/getting-started.md) - < 30min setup guide (new developers start here!)
- [**Coding Standards**](docs/04-development/coding-standards.md) - C# conventions, SOLID, testing guidelines
- [**Local Development**](docs/04-development/local-development.md) - Docker Compose, hot reload, debugging
- [**Git Workflow**](docs/04-development/git-workflow.md) - Branching, commits, PRs

### 🔬 Domain Knowledge

- [**Orbital Mechanics**](docs/05-domain/orbital-mechanics.md) - Hyperbolic orbits, Keplerian elements, 2I/Borisov
- [**Coordinate Systems**](docs/05-domain/coordinate-systems.md) - J2000, ecliptic, transformations
- [**Glossary**](docs/01-overview/glossary.md) - Astronomical & technical terms

### 🚀 Operations

- [**Monitoring**](docs/06-operations/monitoring.md) - Application Insights, Grafana, alerts
- [**Azure Infrastructure**](docs/06-operations/azure-infrastructure.md) - Deployed resources, Terraform
- [**Troubleshooting**](docs/06-operations/troubleshooting.md) - Common issues, health checks

### 📋 Project Management

- [**Roadmap**](ROADMAP.md) - 7-phase plan to completion
- [**Contributing**](CONTRIBUTING.md) - How to contribute (when repo is public)

## 🧪 Testing

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate coverage report
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport

# Run only integration tests
dotnet test --filter Category=Integration
```

### Test Strategy

- **Unit Tests** (17) - TrajectoryService business logic with mocked dependencies
- **Integration Tests** (6) - HTTP client tests using **WireMock.Net** for mocking external CalculationService
- **Test Coverage** - Target **>80%** (Phase 6)

**WireMock Integration**: Integration tests use `WireMock.Net` to mock HTTP responses from CalculationService, following best practices:

- `CustomWebApplicationFactory` - Overrides configuration to point to WireMock server
- `CalculationServiceMock` - Pre-configured HTTP stubs matching real API contracts
- `IAsyncLifetime` pattern - Proper lifecycle management with random port assignment

See [ADR-007](docs/03-adr/007-calculation-service-integration-tdd.md) for TDD implementation details.

## 🐳 Docker & Kubernetes

### Build containers

```powershell
docker-compose build
```

### Deploy to local Kubernetes

```powershell
kubectl apply -f k8s/
```

### Push to GitHub Container Registry

```powershell
docker tag interstellar-tracker/api-gateway ghcr.io/YOUR_USERNAME/interstellar-tracker-api-gateway:latest
docker push ghcr.io/YOUR_USERNAME/interstellar-tracker-api-gateway:latest
```

## 🌐 Deployment

### Local (Docker Desktop)

```powershell
docker-compose up
```

### Azure Kubernetes Service (AKS)

See [deployment documentation](docs/deployment.md) for full Azure setup with:

- Azure Kubernetes Service (AKS)
- Azure Container Registry
- Azure AD B2C for social authentication
- Application Insights for monitoring

## 🤝 Contributing

We welcome contributions! This project is designed to be learning-friendly.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## 📊 Project Status

**Current Phase**: 1 - Documentation (~35-40% complete overall)

| Phase | Component | Status | Coverage |
|-------|-----------|--------|----------|
| ✅ 0 | Infrastructure | 80% | Azure deployed, Terraform, App Insights |
| ✅ 0 | CalculationService | 70% | Hyperbolic orbits, tests |
| ✅ 0 | API Gateway (YARP) | 60% | Routing, telemetry |
| 📝 1 | **Documentation** | **IN PROGRESS** | System overview, ADRs, getting-started |
| ⏳ 2 | VisualizationService | 0% | Planned |
| ⏳ 3 | Microservices Decomposition | 0% | Planned |
| ⏳ 4 | Event-Driven (RabbitMQ) | 0% | Planned |
| ⏳ 5 | SonarQube Quality Gates | 0% | Planned |
| ⏳ 6 | TDD + 80% Coverage | 30% | Target 80% |
| ⏳ 7 | General Review | 0% | Final phase |

**Progress**: [ROADMAP.md](ROADMAP.md) | **Azure**: [Portal Dashboard](https://portal.azure.com/)

### Test Coverage

- **Current**: ~30% (xUnit tests in Domain, Application)
- **Target**: >80% (Phase 6 - TDD adoption)
- **Quality Gate**: SonarQube (Phase 5)

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **2I/Borisov orbital data** - [JPL Small-Body Database](https://ssd.jpl.nasa.gov/)
- **Silk.NET** - Modern .NET OpenGL bindings
- **Keycloak** - Open-source identity and access management
- **Clean Architecture** - Robert C. Martin's architectural pattern

## 📧 Contact

- **Project Maintainer** - [@YOUR_USERNAME](https://github.com/YOUR_USERNAME)
- **Email** - <eprey27@gmail.com>
- **Issues** - [GitHub Issues](https://github.com/YOUR_USERNAME/interstellar-tracker/issues)

---

**Built with ❤️ for learning and exploring the cosmos** 🚀
