# ADR-004: Authentication Provider Selection

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires enterprise-grade authentication supporting:

- Single Sign-On (SSO) for ~3,000+ users across 136 care centers
- Multi-factor authentication (MFA) for PHI access
- Role-based access control (RBAC) with program-level scoping
- SAML integration for enterprise identity providers
- OIDC for modern web application authentication
- Compliance with HIPAA security requirements
- User lifecycle management

## Decision

We will use **Okta** as the identity provider with OIDC for the React SPA and JWT validation for the API.

**Configuration:**
- Protocol: OpenID Connect (OIDC) with Authorization Code + PKCE flow
- SAML 2.0: Available for enterprise SSO federation
- MFA: Enforced via Okta policies for all users
- Token lifetime: Access Token 1 hour, Refresh Token 8 hours
- Claims: sub, email, name, groups (mapped to application roles)

**SDK versions:**
- Frontend: @okta/okta-react 6.7.0, @okta/okta-auth-js 7.5.1
- Backend: Okta.AspNetCore 4.5.0

**RBAC Implementation:**
- Okta groups mapped to application roles
- Roles: SystemAdmin, FoundationAnalyst, ProgramAdmin, ClinicalUser, ReadOnlyUser
- Program-level scoping via custom claims or database lookup

## Consequences

### Positive
- **Mandated by tech stack**: Aligns with Foundation's required technology stack
- **Enterprise SSO**: Native SAML and OIDC support for federation
- **MFA**: Built-in MFA with multiple factor options
- **Compliance**: SOC 2, HIPAA, and other compliance certifications
- **User management**: Self-service password reset, lifecycle management
- **SDK support**: Official SDKs for React and ASP.NET Core
- **Scalability**: Cloud-native, handles enterprise user volumes
- **Security**: Regular security updates, threat detection

### Negative
- **Cost**: Per-user licensing costs
- **Dependency**: External service dependency for authentication
- **Complexity**: Requires Okta administration expertise
- **Latency**: Token validation adds network latency (mitigated by caching)

### Risks
- **Service availability**: Okta outage would prevent user authentication
- **Configuration errors**: Misconfigured policies could lock out users
- **Token management**: Must handle token refresh and revocation properly

## Alternatives Considered

### Azure AD B2C
- **Rejected**: Not in mandated tech stack; Okta specifically required

### Auth0
- **Rejected**: Not in mandated tech stack; similar capabilities but Okta specified

### Custom JWT implementation
- **Rejected**: Higher security risk; significant development effort; no SSO support
