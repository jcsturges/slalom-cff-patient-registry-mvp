# ADR-006: Authentication Strategy - Azure AD SSO with OIDC

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR must implement Single Sign-On (SSO) for all users across 136 care centers and Foundation staff. The authentication system must support HIPAA compliance, integrate with existing identity providers, and provide secure session management.

## Decision

We will use **Azure Active Directory (Azure AD)** as the identity provider with **OpenID Connect (OIDC)** protocol for authentication.

## Consequences

### Positive
- Native integration with Foundation's existing Microsoft ecosystem
- Enterprise-grade security with MFA support
- Centralized user management and provisioning
- HIPAA-compliant with proper configuration
- Supports B2B collaboration for care center users
- Built-in audit logging for authentication events
- Conditional access policies for additional security
- Well-documented integration patterns for .NET and React

### Negative
- Dependency on Microsoft identity infrastructure
- Care centers may need Azure AD B2B setup
- Some complexity in multi-tenant scenarios

### Risks
- Care center identity federation complexity
- Mitigation: Use Azure AD B2B for external users; provide clear onboarding documentation

## Alternatives Considered

### Auth0
- **Pros:** Developer-friendly, flexible, good documentation
- **Cons:** Additional cost, another vendor relationship, not native to Azure
- **Rejected because:** Azure AD provides equivalent functionality with better Azure integration

### Okta
- **Pros:** Enterprise-grade, excellent SSO capabilities, wide protocol support
- **Cons:** Additional cost, separate identity silo, integration overhead
- **Rejected because:** Foundation already has Azure AD; adding Okta creates unnecessary complexity

### Keycloak (Self-Hosted)
- **Pros:** Open source, full control, no licensing costs
- **Cons:** Operational overhead, security responsibility, scaling complexity
- **Rejected because:** Managed identity service preferred for security-critical healthcare application

### Custom JWT Implementation
- **Pros:** Full control, no external dependencies
- **Cons:** Security risks, maintenance burden, reinventing the wheel
- **Rejected because:** Authentication is security-critical; proven solutions preferred
