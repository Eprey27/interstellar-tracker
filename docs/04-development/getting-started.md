# Getting Started

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Development Environment](#development-environment)
- [Running Tests](#running-tests)
- [Common Tasks](#common-tasks)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required

- **.NET 8.0 SDK** or later ([download](https://dotnet.microsoft.com/download))
- **Docker Desktop** (for local infrastructure)
- **Git** (for version control)
- **VS Code** or **Visual Studio 2022**

### Recommended

- **Azure CLI** (for cloud deployment)
- **Terraform** (for infrastructure)
- **PowerShell Core** (for scripts)

### Verification

```powershell
# Check .NET version
dotnet --version  # Should be >= 8.0

# Check Docker
docker --version
docker-compose --version

# Check Git
git --version
```

## Quick Start

**Target: < 30 minutes from clone to running system**

### 1. Clone Repository

```powershell
git clone https://github.com/your-org/interstellar-tracker.git
cd interstellar-tracker
```

### 2. Start Infrastructure

```powershell
# Start Keycloak, RabbitMQ, Prometheus, Grafana
docker-compose up -d

# Verify containers
docker ps
```

Expected containers:

- `keycloak` (port 8080)
- `postgres` (port 5432)
- `rabbitmq` (port 5672, management 15672)
- `prometheus` (port 9090)
- `grafana` (port 3000)

### 3. Restore Dependencies

```powershell
dotnet restore InterstellarTracker.sln
```

### 4. Build Solution

```powershell
dotnet build InterstellarTracker.sln
```

### 5. Run Tests

```powershell
dotnet test InterstellarTracker.sln
```

### 6. Start Services

#### Option A: VS Code Tasks

```
Ctrl+Shift+P → "Tasks: Run Task" → "docker-up"
```

#### Option B: Manual

```powershell
# Terminal 1: CalculationService
cd src/Services/CalculationService/InterstellarTracker.CalculationService
dotnet run

# Terminal 2: API Gateway
cd src/Services/ApiGateway/InterstellarTracker.ApiGateway
dotnet run

# Terminal 3: WebUI
cd src/Web/InterstellarTracker.WebUI
dotnet run
```

### 7. Verify Services

- **CalculationService**: <http://localhost:5001/swagger>
- **API Gateway**: <http://localhost:5014/swagger>
- **WebUI**: <http://localhost:5015>
- **RabbitMQ Management**: <http://localhost:15672> (guest/guest)
- **Grafana**: <http://localhost:3000> (admin/admin)

## Development Environment

### VS Code Extensions

- C# Dev Kit
- Docker
- Azure Tools
- GitLens
- REST Client (for API testing)

### VS Code Settings

```json
{
  "dotnet.defaultSolution": "InterstellarTracker.sln",
  "omnisharp.enableEditorConfigSupport": true,
  "csharp.format.enable": true
}
```

### User Secrets (Local Development)

```powershell
cd src/Services/CalculationService/InterstellarTracker.CalculationService

dotnet user-secrets set "ApplicationInsights:ConnectionString" "your-connection-string"
dotnet user-secrets set "AzureKeyVault:VaultUri" "https://your-vault.vault.azure.net/"
```

## Running Tests

### All Tests

```powershell
dotnet test InterstellarTracker.sln
```

### Specific Test Project

```powershell
dotnet test tests/Domain.Tests/InterstellarTracker.Domain.Tests/
```

### With Coverage

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

Coverage report: `tests/**/TestResults/**/coverage.cobertura.xml`

### Watch Mode (TDD)

```powershell
dotnet watch test --project tests/Domain.Tests/InterstellarTracker.Domain.Tests/
```

## Common Tasks

### Add New Package

```powershell
cd src/Application/InterstellarTracker.Application
dotnet add package FluentValidation
```

### Create Migration (if using EF Core)

```powershell
cd src/Infrastructure/InterstellarTracker.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Generate API Client

```powershell
# From Swagger/OpenAPI spec
# TODO: Add nswag or openapi-generator commands
```

### Update Terraform

```powershell
cd terraform/environments/dev
terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

## Troubleshooting

### Docker containers not starting

```powershell
# Check logs
docker-compose logs keycloak

# Restart specific service
docker-compose restart keycloak

# Nuclear option
docker-compose down -v
docker-compose up -d
```

### Build errors

```powershell
# Clean solution
dotnet clean InterstellarTracker.sln

# Clear NuGet cache
dotnet nuget locals all --clear

# Restore and rebuild
dotnet restore
dotnet build
```

### Port already in use

```powershell
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process (use PID from above)
taskkill /PID <pid> /F
```

### Tests failing

```powershell
# Verbose test output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~HyperbolicOrbitTests.Calculate_Position_WithValidParameters"
```

### Application Insights not receiving data

1. Verify connection string in user secrets
2. Check `appsettings.Development.json`
3. Enable logging:

   ```json
   "Logging": {
     "ApplicationInsights": {
       "LogLevel": { "Default": "Debug" }
     }
   }
   ```

## Next Steps

1. Read [Architecture Overview](../02-architecture/system-overview.md)
2. Review [Coding Standards](./coding-standards.md)
3. Check [Current Roadmap](../../ROADMAP.md)
4. Explore [Domain Concepts](../05-domain/orbital-mechanics.md)

## Getting Help

- **Internal Docs**: `docs/` folder
- **Issues**: GitHub Issues tracker
- **ADRs**: `docs/adr/` for architecture decisions
- **Team Chat**: [Slack/Teams link]
