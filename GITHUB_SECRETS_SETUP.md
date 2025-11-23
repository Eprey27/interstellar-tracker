# Configure GitHub Secrets - Step by Step

## Security Notice

**IMPORTANT**: Never share tokens in chat, email, or documentation.

Tokens should only be stored in:

- GitHub Secrets (encrypted)
- Azure Key Vault (production)
- Local .env files (development, gitignored)

---

## Steps to Configure SONAR_TOKEN in GitHub Secrets

### Step 1: Regenerate the Token (Since it was exposed)

1. Open [SonarCloud](https://sonarcloud.io)
2. Go to Organizations → eprey → Administration → Security → Tokens
3. Find "GitHub Token for CI/CD 30 days"
4. Click the X button to DELETE it (it's compromised)
5. Click "Generate Tokens"
6. Fill in these details:
   - Name: `GitHub Actions - CI/CD`
   - Type: `Project Analysis` or `Organization`
   - Expiration: 90 days
7. Click "Generate"
8. COPY the token immediately (you won't see it again)

### Step 2: Store Token Securely in GitHub Secrets

1. Go to GitHub → Your Repository
2. Click Settings (top right)
3. Click "Secrets and variables" → "Actions"
4. Click "New repository secret"
5. Enter:
   - Name: `SONAR_TOKEN`
   - Value: Paste the NEW token you just generated
6. Click "Add secret"

### Step 3: Verify Configuration

GitHub will show the secret as stored (masked for security).

### Step 4: Test the Integration

Run this command to trigger the workflow:

```powershell
git push origin feature/iter1.1-architecture-audit
```

---

## Success Indicators

- GitHub Secrets shows SONAR_TOKEN (masked)
- GitHub Actions workflow runs without "403 Forbidden" errors
- SonarCloud dashboard shows new analysis results
- Test coverage and code metrics appear in SonarCloud

---

## Security Best Practices

### Do

- Store tokens in GitHub Secrets (encrypted at rest)
- Regenerate tokens if exposed
- Use short expiration times (30-90 days)
- Use minimal permissions (project analysis only)
- Rotate tokens regularly

### Don't

- Share tokens in chat, email, or Slack
- Commit tokens to code repository
- Include tokens in documentation
- Use tokens with excessive permissions
- Leave expired tokens active

---

## Troubleshooting

If the workflow fails after configuring SONAR_TOKEN:

1. Check GitHub Actions → Code Quality Analysis workflow
2. Look at the "SonarQube Scan" step output
3. Common errors and solutions:
   - **403 Forbidden**: Token is invalid or expired (regenerate it)
   - **SONAR_TOKEN not configured**: Secret name is wrong (must be `SONAR_TOKEN` exactly)
   - **Repository key not found**: Project key mismatch in workflow

---

**Once you've regenerated the token and added it to GitHub Secrets, push the code and we'll verify everything works!**
