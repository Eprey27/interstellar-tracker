# ADR 003: Keycloak for Authentication and Authorization

**Date:** 2025-11-22  
**Status:** Accepted  
**Deciders:** Architecture Team

## Context

We need enterprise-grade authentication and authorization that supports:
- Local development with username/password
- Production with social login (Google, GitHub, Microsoft)
- API authentication (JWT tokens)
- Future multi-tenancy support
- Integration with .NET microservices

## Decision

We will use **Keycloak 26.x** as our Identity and Access Management (IAM) solution.

### Architecture

```
┌─────────────┐
│   Clients   │ (Web, API)
└──────┬──────┘
       │ 1. Request protected resource
       ▼
┌─────────────┐
│ API Gateway │
└──────┬──────┘
       │ 2. Validate JWT
       ▼
┌─────────────┐
│ Auth Service│ ◄──────┐
└──────┬──────┘        │ 3. Check token
       │               │
       ▼               │
┌─────────────┐        │
│  Keycloak   │────────┘
│   (IdP)     │ ◄────── 4. Social OAuth
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ PostgreSQL  │ (User data, sessions)
└─────────────┘
```

## Configuration

### Development
- **URL:** http://localhost:8080
- **Realm:** interstellar-tracker
- **Admin:** admin / admin
- **Database:** PostgreSQL 17

### Clients
1. **interstellar-api** - Backend API (confidential)
2. **interstellar-web** - Frontend app (public)

### Social Providers (Production)
- Google OAuth 2.0
- GitHub OAuth
- Microsoft Entra ID (Azure AD)

## Consequences

### Positive

✅ **Industry Standard** - Widely adopted, production-proven  
✅ **Feature Rich** - Social login, SSO, MFA, user federation  
✅ **Extensible** - Custom themes, extensions, SPIs  
✅ **Standard Protocols** - OAuth 2.0, OpenID Connect, SAML  
✅ **Multi-Tenancy** - Realms for different environments  
✅ **Admin UI** - Comprehensive management console  
✅ **Free & Open Source** - No licensing costs

### Negative

❌ **Complexity** - Learning curve for configuration  
❌ **Resource Usage** - JVM-based, requires memory  
❌ **Setup Time** - Initial configuration required  
❌ **Documentation** - Can be overwhelming

### Mitigations

- Provide pre-configured Keycloak realm export
- Document common scenarios for junior developers
- Use Docker Compose for easy local setup
- Create initialization scripts for realms/clients

## Implementation Details

### JWT Token Flow

1. User logs in via Keycloak
2. Keycloak returns JWT access token
3. Client includes token in `Authorization: Bearer {token}`
4. API Gateway validates token (signature, expiry, claims)
5. Request forwarded to microservice with user context

### Token Validation

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/interstellar-tracker";
        options.Audience = "interstellar-api";
        options.RequireHttpsMetadata = false; // Dev only
    });
```

### Roles & Permissions

- **user** - View solar system, basic features
- **premium** - Time acceleration, advanced features
- **admin** - Full access, configuration

## Security Considerations

🔒 **Development**
- HTTP allowed for localhost
- Self-signed certificates
- Default admin credentials documented

🔒 **Production**
- HTTPS required (Let's Encrypt + Azure Front Door)
- Strong admin passwords (Key Vault)
- Social login only (no username/password)
- Token rotation and refresh
- Rate limiting on API Gateway

## Alternatives Considered

1. **Azure AD B2C**
   - Cloud-only, vendor lock-in
   - More expensive at scale
   - Chosen for production Azure deployment

2. **Auth0**
   - SaaS only, costs money
   - Great UX but less control

3. **IdentityServer (Duende)**
   - .NET native, but paid licensing
   - More code to maintain

4. **Custom JWT Auth**
   - Too much work, security risks
   - Don't roll your own crypto

## Migration Path to Azure

For Azure deployment, we'll use **Azure AD B2C** for social login while keeping Keycloak architecture:

- Local/Dev: Keycloak
- Production: Azure AD B2C
- Auth Service abstracts the difference

## References

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [OpenID Connect Spec](https://openid.net/specs/openid-connect-core-1_0.html)
- [OAuth 2.0 RFC](https://datatracker.ietf.org/doc/html/rfc6749)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
