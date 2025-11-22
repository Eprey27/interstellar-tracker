# 🌌 Interstellar Tracker

> A professional 3D visualization system for tracking the passage of interstellar object **2I/Borisov** through our solar system.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
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

This project follows **Clean Architecture** with microservices pattern:

```
┌─────────────────────────────────────────┐
│         API Gateway (Ocelot)            │
└─────────────┬───────────────────────────┘
              │
    ┌─────────┼─────────┬─────────────┐
    │         │         │             │
┌───▼───┐ ┌──▼──┐ ┌────▼─────┐ ┌────▼──────┐
│ Auth  │ │Calc │ │   Viz    │ │    Web    │
│Service│ │Svc  │ │ Service  │ │  Client   │
└───┬───┘ └──┬──┘ └────┬─────┘ └────┬──────┘
    │        │         │             │
    └────────┴─────────┴─────────────┘
              │
    ┌─────────▼──────────┐
    │  Infrastructure    │
    │ (Domain/App Logic) │
    └────────────────────┘
```

### Layer Structure

- **Domain** - Core business models (CelestialBody, Orbit, InterstellarObject)
- **Application** - Use cases and business logic
- **Infrastructure** - Data persistence, external services
- **Services** - Microservices (API Gateway, Auth, Calculation, Visualization)
- **Web** - 3D visualization frontend (Silk.NET + OpenGL)

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop) with Kubernetes enabled
- [Git](https://git-scm.com/)
- Visual Studio 2022, VS Code, or Rider

### Local Development Setup

1. **Clone the repository**

   ```powershell
   git clone https://github.com/YOUR_USERNAME/interstellar-tracker.git
   cd interstellar-tracker
   ```

2. **Start infrastructure services**

   ```powershell
   docker-compose up -d
   ```

   This starts:
   - PostgreSQL 17 (port 5432)
   - Keycloak 26 (port 8080)
   - MailHog (SMTP: 1025, Web: 8025)
   - Prometheus (port 9090)
   - Grafana (port 3000, admin/admin)

3. **Build the solution**

   ```powershell
   dotnet restore
   dotnet build
   ```

4. **Run tests**

   ```powershell
   dotnet test
   ```

5. **Start services** (in separate terminals)

   ```powershell
   # API Gateway
   dotnet run --project src/Services/ApiGateway/InterstellarTracker.ApiGateway

   # Other services...
   # (See docs/development.md for full setup)
   ```

6. **Launch 3D visualization**

   ```powershell
   dotnet run --project src/Web/InterstellarTracker.Web
   ```

## 📖 Documentation

- [Architecture Decision Records](docs/adr/) - Key architectural decisions
- [Development Guide](docs/development.md) - Detailed setup and workflows
- [API Documentation](docs/api.md) - REST API endpoints
- [Deployment Guide](docs/deployment.md) - Kubernetes and Azure deployment
- [Contributing Guidelines](CONTRIBUTING.md) - How to contribute

## 🧪 Testing

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate coverage report
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport
```

Target: **>80% code coverage**

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

- [x] Project structure and workspace setup
- [ ] Domain models implementation
- [ ] Unit testing infrastructure
- [ ] Microservices development
- [ ] 3D visualization engine
- [ ] CI/CD pipeline
- [ ] Kubernetes deployment
- [ ] Azure production deployment

See [GitHub Projects](https://github.com/YOUR_USERNAME/interstellar-tracker/projects) for detailed progress.

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
