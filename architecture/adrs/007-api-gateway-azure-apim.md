# ADR-007: API Gateway - Azure API Management

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR exposes multiple backend services that need unified API management including authentication, rate limiting, versioning, and monitoring. The gateway must support future API consumers (EMR integrations, patient portal) while providing security controls.

## Decision

We will use **Azure API Management (APIM)** as the API gateway for all backend services.

## Consequences

### Positive
- Native Azure integration with Azure AD, Key Vault, and Monitor
- Built-in rate limiting and throttling
- API versioning support
- Developer portal for API documentation
- Request/response transformation capabilities
- Comprehensive analytics and logging
- Policy-based security controls
- Supports future external API exposure

### Negative
- Additional cost component
- Some latency added to requests
- Learning curve for policy configuration

### Risks
- APIM as single point of failure
- Mitigation: Deploy in multiple availability zones; implement circuit breakers in services

## Alternatives Considered

### Kong Gateway
- **Pros:** Open source option, plugin ecosystem, Kubernetes-native
- **Cons:** Operational overhead, separate infrastructure, less Azure integration
- **Rejected because:** Azure APIM provides equivalent features with managed service benefits

### AWS API Gateway
- **Pros:** Mature service, good documentation
- **Cons:** Wrong cloud provider, would require cross-cloud networking
- **Rejected because:** Not compatible with Azure-first strategy

### Nginx/Envoy (Self-Managed)
- **Pros:** High performance, full control, no licensing
- **Cons:** Operational burden, manual scaling, security responsibility
- **Rejected because:** Managed service preferred for security-critical gateway

### Direct Service Exposure
- **Pros:** Simpler architecture, lower latency
- **Cons:** No centralized security, no rate limiting, harder to version
- **Rejected because:** Lacks enterprise API management features needed for healthcare application
