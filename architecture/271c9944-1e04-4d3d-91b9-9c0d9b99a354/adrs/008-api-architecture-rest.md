# ADR-008: API Architecture - RESTful APIs with OpenAPI

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires APIs for communication between the frontend application and backend services, as well as for future external integrations. The API architecture must:
- Support the web application's data needs
- Enable future EMR and external system integrations
- Be well-documented for internal and external developers
- Support versioning for backward compatibility
- Follow healthcare interoperability standards where applicable

## Decision

We will implement **RESTful APIs** with **OpenAPI 3.0 specifications** for all services.

Design principles:
- Resource-oriented URLs (`/patients/{id}`, `/forms/{id}/submissions`)
- Standard HTTP methods (GET, POST, PUT, PATCH, DELETE)
- JSON request/response bodies
- HTTP status codes for error handling
- API versioning via URL path (`/api/v1/...`)
- OpenAPI 3.0 specs generated from code annotations
- HATEOAS links for discoverability (where beneficial)

## Consequences

### Positive
- **Simplicity**: REST is well-understood and widely adopted
- **Tooling**: Excellent tooling for documentation, testing, code generation
- **Caching**: HTTP caching semantics for performance optimization
- **Stateless**: Stateless design enables horizontal scaling
- **Documentation**: OpenAPI provides interactive documentation (Swagger UI)
- **Future-Ready**: RESTful design can evolve toward FHIR for healthcare interoperability
- **Client Flexibility**: Any HTTP client can consume the API

### Negative
- **Over-fetching**: REST can return more data than needed
- **Multiple Requests**: Complex views may require multiple API calls
- **Versioning Overhead**: Breaking changes require version management
- **No Real-time**: REST alone doesn't support real-time updates

### Mitigations
- Implement sparse fieldsets (`?fields=id,name`) for response optimization
- Use composite endpoints for common multi-resource views
- Establish clear versioning and deprecation policies
- Consider WebSocket or SSE for future real-time features

## Alternatives Considered

### GraphQL
- **Pros**: Flexible queries, single endpoint, strong typing
- **Cons**: Complexity, caching challenges, learning curve, security considerations
- **Rejected**: Added complexity not justified for current requirements

### gRPC
- **Pros**: High performance, strong typing, streaming support
- **Cons**: Browser support requires proxy, less human-readable, smaller ecosystem
- **Rejected**: REST's simplicity and browser compatibility preferred

### FHIR (Fast Healthcare Interoperability Resources)
- **Pros**: Healthcare standard, interoperability, rich data model
- **Cons**: Complex specification, overhead for internal APIs, learning curve
- **Rejected for MVP**: Design REST APIs with FHIR compatibility in mind for future

### OData
- **Pros**: Standardized query syntax, filtering, pagination
- **Cons**: Complex specification, limited adoption outside Microsoft ecosystem
- **Rejected**: Standard REST with custom query parameters sufficient
