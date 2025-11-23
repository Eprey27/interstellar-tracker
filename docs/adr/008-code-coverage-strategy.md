# ADR 008: Code Coverage Strategy & Reporting

**Date:** 2025-11-23  
**Status:** PROPOSED  
**Context:** Iteration 1.1 - Architecture Audit Phase  
**Deciders:** Development Team  

## Problem Statement

The project currently lacks integrated code coverage reporting across the CI/CD pipeline. We need a strategy to:

1. Track code coverage metrics consistently
2. Enforce coverage gates in CI/CD
3. Provide visibility across GitHub and SonarQube
4. Establish minimum coverage thresholds
5. Generate historical coverage trends

## Objectives

### Phase 1: Current (Iteration 1.1)

- ✅ Integrate XPlat Code Coverage in GitHub Actions workflow
- ✅ Upload coverage reports to SonarQube Cloud (Pro trial)
- ✅ Establish baseline coverage metrics
- ✅ Document coverage standards in README

### Phase 2: Next Iteration

- 🎯 Enforce minimum coverage threshold (80%) in workflow
- 🎯 Add codecov.io integration for PR comments
- 🎯 Generate coverage badges for README
- 🎯 Configure SonarQube quality gates based on coverage

### Phase 3: Future (Post-Infrastructure)

- 🎯 Migrate to self-hosted SonarQube in Azure
- 🎯 Add coverage trend analysis
- 🎯 Integrate with development dashboard

## Architecture Decision

### Coverage Collection

```
dotnet test → XPlat Code Coverage → coverage.cobertura.xml
                                  ↓
                    GitHub Actions Artifacts
                                  ↓
                    ┌─────────────┴──────────────┐
                    ↓                            ↓
            SonarQube Cloud              CodeCov.io (Phase 2)
                    ↓                            ↓
            Coverage Dashboard        PR Comments + Badges
```

### Tools & Integration

| Component | Tool | Purpose | Cost |
|-----------|------|---------|------|
| **Collection** | XPlat Code Coverage | Generates Cobertura XML reports | Free (built-in) |
| **CI/CD Analysis** | SonarQube Cloud (Pro) | Analyzes coverage + quality | $10/mo (trial free) |
| **PR Reporting** | CodeCov.io (Phase 2) | Comments on PRs with delta | Free tier available |
| **Artifacts** | GitHub Actions | Stores coverage reports | Free |
| **Trends** | SonarQube Dashboard | Historical tracking | Pro feature |

### Coverage Standards (Target)

```
Codebase Layer          Target Coverage    Priority
────────────────────────────────────────────────────
Domain (Business Logic)      ≥ 90%         CRITICAL
Application (Use Cases)      ≥ 85%         HIGH
Infrastructure (Repos)       ≥ 70%         MEDIUM
Services (Controllers)       ≥ 75%         HIGH
Web/UI (Rendering)           ≥ 50%         LOW
Tests (Test utilities)       N/A           EXCLUDED
```

### Exclusions

Files to exclude from coverage (not critical for coverage %):

```csharp
// Test infrastructure
**/*Tests.cs
**/*Mock.cs
**/*Fixtures.cs

// Configuration & Scaffolding
Program.cs (partial, ASP.NET convention)
**/*.razor (UI components, covered via integration tests)
```

## Implementation Details

### GitHub Actions Workflow

```yaml
- name: Run tests with coverage
  run: dotnet test --configuration Release --no-build 
       --logger "trx" 
       --collect:"XPlat Code Coverage" 
       --verbosity minimal

- name: Upload coverage to SonarQube
  env:
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
  run: |
    # Coverage reports automatically included in SonarQube analysis
    # Reports location: **/coverage.cobertura.xml
```

### SonarQube Configuration

```properties
# .github/sonar-project.properties
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
sonar.coverage.exclusions=**Tests.cs,**Mock.cs
sonar.core.codeCoveragePlugin=cobertura
```

### Quality Gates (Phase 2)

```
Condition                              Operator    Value
────────────────────────────────────────────────────────
New Code Coverage                      <           80%
Code Coverage                          <           70%
New Code Duplication                   >           3%
Maintainability Rating                 <           A
```

## Benefits

### Short-term (Now)

✅ **Visibility** - See what code is tested  
✅ **Baseline** - Measure starting point  
✅ **Awareness** - Team sees coverage daily  

### Medium-term (Phase 2)

✅ **Enforcement** - Coverage gates block PRs  
✅ **Trend Analysis** - Historical tracking  
✅ **Accountability** - Visibility per PR  

### Long-term (Phase 3)

✅ **Scalability** - Works across all projects  
✅ **Cost-effective** - Self-hosted vs SaaS  
✅ **Integration** - Part of development dashboard  

## Consequences

### Positive

✅ Forces developers to write tests  
✅ Catches untested code paths  
✅ Improves overall code quality  
✅ Reduces production defects  
✅ Provides metric for project health  

### Challenges

⚠️ Coverage alone doesn't guarantee quality (% can be misleading)  
⚠️ Requires discipline to maintain standards  
⚠️ Can slow down fast iteration if gates are too strict  
⚠️ Initial effort to reach baseline 80%  

### Mitigation

- 🛡️ Start with monitoring-only (Phase 1)
- 🛡️ Gradually enforce gates (Phase 2)
- 🛡️ Focus on critical paths first (Domain layer)
- 🛡️ Allow exemptions for UI/rendering code

## Current State (Iteration 1.0)

✅ **Already Done:**

- XPlat Code Coverage integrated in workflow
- Coverage reports generated: `coverage.cobertura.xml`
- Artifacts uploaded to GitHub Actions
- SonarQube analyzing coverage metrics

📊 **Current Baseline (from latest run):**

- Tests Executed: 81
- Tests Passed: 81 (100%)
- Coverage Data: Being collected
- Coverage Gate: Not enforced yet

## Migration Path (Future)

### When moving to Self-Hosted SonarQube (Azure)

```
Current (SonarCloud)           →    Future (Self-Hosted + Azure)
───────────────────────────────────────────────────────────────
SonarQube Cloud                →    SonarQube Community Edition
GitHub Actions (manual)        →    GitHub Actions (manual)
Coverage.cobertura.xml         →    Same format (no change)
SonarCloud Dashboard           →    Azure-hosted SonarQube Dashboard
$10/mo (Pro trial)             →    ~$5-15/mo (Azure infra)
```

## References

- **SonarQube Coverage:** <https://docs.sonarsource.com/sonarqube/latest/analyzing-source-code/test-coverage/overview/>
- **Cobertura Format:** <https://cobertura.github.io/>
- **XPlat Code Coverage:** <https://github.com/coverlet-coverage/coverlet>
- **Quality Gates:** <https://docs.sonarsource.com/sonarqube/latest/user-guide/quality-gates/>

## Next Steps

1. ✅ Phase 1: Monitor coverage metrics (THIS ITERATION)
2. 🎯 Phase 2: Enforce coverage gates (next iteration)
3. 🎯 Phase 3: Migrate to self-hosted SonarQube (future)

---

**ADR Status:** PROPOSED → ACCEPTED (after Iteration 1.1 completion)
