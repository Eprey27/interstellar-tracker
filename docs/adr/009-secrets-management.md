# ADR-009: Secrets Management Strategy

**Date**: 2025-11-23  
**Status**: Proposed  
**Context**: Phase 3 Security Hardening  

## Problem

The application handles sensitive configuration (API keys, database connections, authentication tokens) that must:

1. Never be committed to version control
2. Be environment-specific (dev, staging, production)
3. Be rotatable without code changes
4. Be accessible to CI/CD pipelines
5. Comply with security best practices

Current state: ConnectionStrings are empty in source code, but strategy needs formalization.

## Decision

Implement a three-tier secrets management approach:

### 1. Local Development

- Use `appsettings.Development.json` (gitignored)
- Use `.env` files with detect-secrets pre-commit hook
- Never store real credentials locally

### 2. CI/CD Pipelines (GitHub Actions)

- Use GitHub Repository Secrets for:
  - `SONAR_TOKEN`: SonarQube authentication
  - `AZURE_SUBSCRIPTION_ID`: Azure deployment
  - `DOCKER_REGISTRY_PASSWORD`: Docker Hub credentials
  - `KEYCLOAK_ADMIN_PASSWORD`: Keycloak setup
- Inject at runtime via environment variables
- Reference in workflow: `${{ secrets.SECRET_NAME }}`

### 3. Production/Cloud

- Use Azure Key Vault for:
  - Database connection strings
  - Application Insights instrumentation keys
  - API credentials for external services
  - Certificates and signing keys
- Access via Azure Managed Identity (no credentials stored)
- Fallback: Environment variables in Azure App Configuration

## Implementation

### Configuration Pattern (Already Implemented)

```csharp
// In appsettings.json
"ApplicationInsights": {
  "ConnectionString": ""  // Empty in source, injected at runtime
}

// In Program.cs
builder.Services.AddSingleton<IServiceConfiguration, ServiceConfiguration>();
// Configuration loaded from environment variables or Key Vault
```

### GitHub Actions Workflow Template

```yaml
env:
  SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
  AZURE_CREDENTIALS: ${{ secrets.AZURE_CREDENTIALS }}

steps:
  - name: Build with secrets
    run: dotnet build
    env:
      ApplicationInsights__ConnectionString: ${{ secrets.APPINSIGHTS_INSTRUMENTATION_KEY }}
```

### Local Development Setup

```bash
# Create local override file (NEVER COMMIT)
echo "appsettings.Development.json" >> .gitignore

# Create local config with real credentials
cat > appsettings.Development.json << 'EOF'
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=YOUR_KEY_HERE;"
  }
}
EOF
```

### Pre-commit Hook (detect-secrets)

Install locally:

```bash
pip install detect-secrets
detect-secrets scan --baseline .secrets.baseline
```

Add to git hooks:

```bash
git hook add pre-commit detect-secrets
```

## Consequences

### Positive

- Secrets never stored in git history
- Easy rotation without code changes
- Environment-specific configurations
- CI/CD pipelines fully automated
- Production uses managed identity (no secrets in code)

### Negative

- Requires manual setup for local development
- Additional complexity in deployment configuration
- Team must follow practices consistently

## Related ADRs

- ADR-007: Application Insights Setup
- ADR-003: Keycloak Authentication

## References

- [Microsoft: Azure Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [GitHub: Using secrets in workflows](https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions)
- [OWASP: Secrets Management](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)

## Next Steps

1. [ ] Setup GitHub Repository Secrets for SONAR_TOKEN and AZURE credentials
2. [ ] Create Azure Key Vault instance (Phase 4)
3. [ ] Implement Azure Managed Identity authentication (Phase 4)
4. [ ] Add pre-commit hooks to detect-secrets (Phase 4)
5. [ ] Rotate all existing credentials (immediate)
6. [ ] Document local development setup in README
