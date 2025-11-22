# Multi-stage Dockerfile for CalculationService
# Optimized for size and build time

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY InterstellarTracker.sln .
COPY src/Domain/InterstellarTracker.Domain/*.csproj src/Domain/InterstellarTracker.Domain/
COPY src/Application/InterstellarTracker.Application/*.csproj src/Application/InterstellarTracker.Application/
COPY src/Infrastructure/InterstellarTracker.Infrastructure/*.csproj src/Infrastructure/InterstellarTracker.Infrastructure/
COPY src/Services/CalculationService/InterstellarTracker.CalculationService/*.csproj src/Services/CalculationService/InterstellarTracker.CalculationService/

# Restore dependencies
RUN dotnet restore src/Services/CalculationService/InterstellarTracker.CalculationService/InterstellarTracker.CalculationService.csproj

# Copy source code
COPY src/ src/

# Build and publish
WORKDIR /src/src/Services/CalculationService/InterstellarTracker.CalculationService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Create non-root user
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --gid 1000 appuser && \
    chown -R appuser:appgroup /app

USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InterstellarTracker.CalculationService.dll"]
