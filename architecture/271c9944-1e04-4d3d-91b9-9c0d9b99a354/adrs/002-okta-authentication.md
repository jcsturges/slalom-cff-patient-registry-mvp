# ADR-002: Okta for Authentication and SSO

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires Single Sign-On (SSO) integration for user authentication across 136 care centers and 3,000+ users. Key requirements include:

- SSO integration with 100% of user authentication (KR from PRD)
- Support for both OIDC and SAML protocols
- Multi-Factor Authentication (MFA) enforcement
- Role-based access control integration
- Enterprise-grade security for PHI access
- Support for federated identity with care center IdPs

The RFP tech stack mandates Okta (OIDC + SAML via Okta SDK).

## Decision

We will use **Okta** as the Identity Provider (IdP) with the following configuration:

1. **Protocol**: OpenID Connect (OIDC) with Authorization Code + PKCE flow
2. **React Integration**: @okta/okta-react (6.8.0) and @okta/okta-auth-js (7.5.1)
3. **API Integration**: Okta.AspNetCore (4.5.0) middleware for JWT validation
4. **MFA**: Enforced via Okta authentication policies
5. **SAML**: Available for enterprise care centers with existing IdPs

### Authentication Flow

```
User → React SPA → Okta (OIDC) → ID Token + Access Token → API (JWT Validation)
```

### Token Configuration
- Access Token Lifetime: 1 hour
- Refresh Token Lifetime: 8 hours (sliding)
- ID Token: Contains user profile and group claims

## Consequences

### Positive
- **Enterprise SSO**: Proven platform used by healthcare organizations
- **HIPAA Compliant**: Okta maintains HIPAA BAA and SOC 2 Type 2
- **MFA Built-in**: Native support for various MFA factors
- **Federation Ready**: Can federate with care center IdPs via SAML
- **SDK Support**: Well-maintained SDKs for React and .NET
- **Audit Logging**: Comprehensive authentication event logging

### Negative
- **External Dependency**: Authentication relies on third-party service
- **Cost**: Per-user licensing adds to operational costs
- **Complexity**: Requires Okta administration expertise
- **Latency**: Token validation requires network calls to Okta

### Mitigations
- Cache Okta JWKS (JSON Web Key Set) locally with appropriate TTL
- Implement token refresh logic to minimize re-authentication
- Configure Okta for high availability (Okta's SLA: 99.99%)
- Document Okta administration procedures for Foundation staff

## Alternatives Considered

### Azure Active Directory B2C
- **Rejected**: Not in approved tech stack
- Would provide similar functionality with Azure-native integration
- Less healthcare-specific compliance documentation

### Auth0
- **Rejected**: Not in approved tech stack
- Similar capabilities to Okta
- Different pricing model

### Custom JWT Authentication
- **Rejected**: Security risk and maintenance burden
- Would require building MFA, session management, etc.
- No enterprise SSO federation capability
