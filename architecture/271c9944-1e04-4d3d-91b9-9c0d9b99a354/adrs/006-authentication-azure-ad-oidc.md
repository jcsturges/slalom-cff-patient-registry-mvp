# ADR-006: Authentication Strategy - Azure AD with OIDC

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires Single Sign-On (SSO) integration for user authentication. The system must:
- Support ~3,000+ care center users across 136 care centers
- Support Foundation internal staff
- Integrate with existing identity infrastructure
- Meet HIPAA authentication requirements
- Support MFA enforcement
- Provide session management

## Decision

We will use **Azure Active Directory** as the identity provider with **OpenID Connect (OIDC)** protocol for authentication, issuing **JWT tokens (RS256)** for API authorization.

## Consequences

### Positive
- Native integration with Foundation's existing Azure AD tenant
- OIDC is industry standard with excellent library support
- Azure AD provides built-in MFA, conditional access, and security monitoring
- JWT tokens enable stateless API authentication
- Azure AD B2B enables external care center user management
- Audit logging built into Azure AD
- Self-service password reset reduces support burden
- Group-based access control aligns with RBAC model

### Negative
- Dependency on Azure AD availability
- Token refresh handling adds client complexity
- Care center users need Azure AD accounts (B2B guest or synced)

### Risks
- Azure AD outage would prevent all authentication
- Token theft could enable unauthorized access (mitigated by short expiry + refresh tokens)
- B2B user provisioning process needs clear definition

## Alternatives Considered

### Okta
- **Pros:** Best-in-class identity platform, excellent healthcare presence, strong MFA
- **Cons:** Additional vendor relationship, cost, requires federation with Azure AD
- **Rejected because:** Azure AD provides equivalent functionality with native integration

### Auth0
- **Pros:** Developer-friendly, flexible, good documentation
- **Cons:** Additional cost, requires integration with Azure AD for SSO
- **Rejected because:** Adds complexity without significant benefit over native Azure AD

### Keycloak (Self-hosted)
- **Pros:** Open source, full control, no per-user licensing
- **Cons:** Operational burden, security responsibility, no SLA
- **Rejected because:** Managed identity service preferred for healthcare compliance

### SAML 2.0 (instead of OIDC)
- **Pros:** Mature protocol, wide enterprise support
- **Cons:** XML-based, more complex, less suited for SPAs and APIs
- **Rejected because:** OIDC is modern standard better suited for React SPA + API architecture

### Custom Authentication
- **Pros:** Full control, no external dependencies
- **Cons:** Security risk, compliance burden, maintenance overhead
- **Rejected because:** Healthcare authentication should use proven, audited solutions
