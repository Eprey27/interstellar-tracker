# Interstellar Tracker - Copilot Instructions

## Project Overview
3D visualization system for tracking interstellar object 2I/Borisov passage through our solar system.

## Architecture
- Clean Architecture with microservices
- .NET 8.0 (or latest LTS)
- OpenGL rendering via Silk.NET
- Keycloak authentication
- Docker + Kubernetes deployment

## Coding Standards
- Follow C# conventions and .NET best practices
- Use async/await for I/O operations
- Implement comprehensive XML documentation
- Write unit tests for all business logic (xUnit)
- Follow SOLID principles
- Use dependency injection

## Project Structure
- `src/Domain/` - Core business models and interfaces
- `src/Application/` - Use cases and application logic
- `src/Infrastructure/` - External concerns (DB, APIs)
- `src/Services/` - Microservices (API Gateway, Auth, Calculation, Visualization)
- `src/Web/` - Frontend with Silk.NET
- `tests/` - Unit and integration tests
- `docker/` - Dockerfiles and compose files
- `k8s/` - Kubernetes manifests
- `.github/workflows/` - CI/CD pipelines

## Documentation Requirements
- Every public class/method needs XML comments
- Complex algorithms need inline explanations
- Architecture decisions go in `docs/adr/`
- Target audience: junior developers

## Quality Gates
- Code coverage > 80%
- No critical security issues
- All tests passing
- No code smells (maintainability rating A)

## Progress Tracking
- [x] Create copilot-instructions.md
- [ ] Scaffold .NET solution structure
- [ ] Setup Docker Compose
- [ ] Implement domain models
- [ ] Add unit testing infrastructure
- [ ] Create microservices
- [ ] Build 3D visualization
- [ ] Configure CI/CD
- [ ] Setup Kubernetes
- [ ] Document everything
