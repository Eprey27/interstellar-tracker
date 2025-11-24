# SonarQube GitHub Secrets Configuration

## Overview

The GitHub Actions workflow requires a valid SonarCloud token to authenticate and analyze code. This document explains how to configure the `SONAR_TOKEN` GitHub Secret.

## Prerequisites

1. SonarCloud account with organization access
2. Repository with GitHub Actions workflow enabled
3. Admin access to the GitHub repository settings

## Step 1: Generate SonarCloud Token

### Option A: Organization Token (Recommended for CI/CD)

1. Go to [SonarCloud](https://sonarcloud.io)
2. Navigate to **Organizations** → Your Organization (e.g., "eprey")
3. Go to **Administration** → **Security** → **Tokens**
4. Click **Generate Tokens**
5. Create a token named "GitHub Actions - interstellar-tracker"
6. Set expiration (e.g., 90 days)
7. Select scope: **Execute Analysis** (minimum required)
8. Click **Generate**
9. **COPY THE TOKEN IMMEDIATELY** (it won't be shown again)

### Option B: User Token (Fallback if organization token fails)

1. Go to [SonarCloud](https://sonarcloud.io)
2. Click your profile → **Security**
3. Go to **Tokens**
4. Click **Generate Tokens**
5. Name: "GitHub Actions CI/CD"
6. Type: **Project Analysis**
7. Copy the token

## Step 2: Configure GitHub Secret

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `SONAR_TOKEN`
5. Value: Paste the token from Step 1
6. Click **Add secret**

## Step 3: Verify Configuration

### Check if Token is Accessible

1. Navigate to `.github/workflows/code-quality.yml`
2. The step "SonarQube Scan" includes a check:

   ```bash
   if (-z "${SONAR_TOKEN}") {
     echo "ERROR: SONAR_TOKEN not configured in GitHub Secrets"
     exit 1
   }
   ```

### Run a Test Analysis

1. Push a commit to `develop` or `feature/*` branch
2. Go to **Actions** tab in GitHub
3. Watch the "Code Quality Analysis" workflow run
4. Check the "SonarQube Scan" step for:
   - ✅ No "403 Forbidden" errors
   - ✅ No "SONAR_TOKEN not configured" errors
   - ✅ Analysis completes successfully

## Troubleshooting

### Error: "403 Forbidden"

**Causes:**

- Token is invalid or expired
- Token doesn't have "Execute Analysis" permission
- SonarCloud organization/project key mismatch

**Solutions:**

1. Verify token is still valid in SonarCloud → Organization → Security → Tokens
2. Check organization key matches workflow: `/o:"eprey"`
3. Check project key matches SonarCloud project: `/k:"Eprey27_interstellar-tracker"`
4. Regenerate token and update GitHub Secret

### Error: "SONAR_TOKEN not configured in GitHub Secrets"

**Causes:**

- Secret name is misspelled
- Secret not set in repository (wrong repository selected)
- Workflow file has incorrect secret reference

**Solutions:**

1. Verify secret name is exactly `SONAR_TOKEN`
2. Verify you configured the secret in the correct repository
3. Verify workflow uses `${{ secrets.SONAR_TOKEN }}`

### Error: "Automatic Analysis is enabled"

**Cause:**

- SonarCloud is running automatic analysis (duplicate scans)

**Solution:**

1. Go to [SonarCloud Projects](https://sonarcloud.io/organizations/eprey/projects)
2. Click project → **Administration** → **Analysis Method**
3. Disable **"Automatic Analysis"**
4. Save changes

## Related Documentation

- [SonarQube Documentation: GitHub Actions Integration](https://docs.sonarqube.org/latest/analysis/github-integration/)
- [SonarCloud: Security Tokens](https://sonarcloud.io/account/security)
- [GitHub Actions: Using Secrets](https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions)

## Validation Checklist

- [ ] SonarCloud token generated with "Execute Analysis" permission
- [ ] GitHub Secret `SONAR_TOKEN` created in repository
- [ ] Organization key in workflow matches SonarCloud org
- [ ] Project key in workflow matches SonarCloud project
- [ ] Automatic Analysis disabled in SonarCloud
- [ ] Test workflow run completed without 403 errors
- [ ] Code analysis results appear in SonarCloud dashboard
