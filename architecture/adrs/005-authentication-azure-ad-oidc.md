# ADR-005: Authentication Strategy - Azure AD with OIDC

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires Single Sign-On (SSO) integration for user authentication. The system must:
- Support ~3,000 care center users across 136 programs
- Support Foundation internal staff
- Integrate with existing identity providers
- Meet HIPAA security requirements
- Support multi-factor authentication (MFA)
- Enable centralized user provisioning and deprovisioning

## Decision

We will use **Azure Active Directory (Azure AD)** as the identity provider with **OpenID Connect (OIDC)** protocol for authentication.

Implementation details:
- OIDC Authorization Code Flow with PKCE for web application
- JWT access tokens for API authentication
- Azure AD groups for initial role assignment hints
- Application-managed RBAC for fine-grained permissions
- Redis-backed session management

## Consequences

### Positive
- **Foundation Alignment**: Foundation already uses Microsoft ecosystem
- **Enterprise Features**: MFA, conditional access, identity protection built-in
- **Standards-Based**: OIDC is industry standard, avoiding vendor lock-in at protocol level
- **User Management**: Centralized user lifecycle management
- **Audit Trail**: Azure AD provides authentication audit logs
- **B2B Support**: Azure AD B2B enables external user collaboration if needed
- **Compliance**: Azure AD has HIPAA BAA coverage

### Negative
- **Azure Dependency**: Ties authentication to Azure ecosystem
- **Complexity**: OIDC flows require careful implementation
- **Cost**: Azure AD Premium features (conditional access) have licensing costs
- **Latency**: External IdP adds network hop to authentication flow

### Mitigations
- Abstract authentication behind service interface for potential future IdP changes
- Implement token caching to reduce IdP round-trips
- Use Azure AD Premium P1 (included in many Microsoft 365 plans) for MFA
- Implement robust token refresh handling

## Alternatives Considered

### Okta
- **Pros**: Leading identity platform, excellent developer experience
- **Cons**: Additional vendor relationship, separate licensing, no existing Foundation relationship
- **Rejected**: Additional cost and complexity when Azure AD is already available

### Auth0
- **Pros**: Developer-friendly, flexible, good documentation
- **Cons**: Additional vendor, cost at scale, recently acquired by Okta
- **Rejected**: Unnecessary additional vendor when Azure AD meets requirements

### Self-Hosted Keycloak
- **Pros**: Open-source, full control, no licensing costs
- **Cons**: Operational overhead, security responsibility, no enterprise support
- **Rejected**: Operational burden and security responsibility concerns for healthcare application

### SAML 2.0
- **Pros**: Widely supported, mature protocol
- **Cons**: XML-based, more complex than OIDC, less suitable for SPAs
- **Rejected**: OIDC is more modern and better suited for SPA architecture
