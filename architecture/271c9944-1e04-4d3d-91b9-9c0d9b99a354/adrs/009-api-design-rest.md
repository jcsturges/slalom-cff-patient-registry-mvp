# ADR-009: API Design - RESTful with OpenAPI 3.0

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires APIs for:
- Frontend web application communication
- Future integrations (EMR, patient portal, wearables)
- Data warehouse export
- Potential third-party access

The API design must support the current MVP requirements while enabling future extensibility for healthcare integrations.

## Decision

We will implement **RESTful APIs** following **OpenAPI 3.0 specification**, with **JSON** as the data format. APIs will be versioned using URL path versioning (e.g., `/api/v1/`).

### Design Principles
- Resource-oriented URLs (`/patients`, `/forms`, `/encounters`)
- Standard HTTP methods (GET, POST, PUT, PATCH, DELETE)
- Consistent error response format
- HATEOAS links for discoverability (where appropriate)
- Pagination for list endpoints
- Filtering and sorting via query parameters

## Consequences

### Positive
- REST is universally understood, reducing integration friction
- OpenAPI enables automatic documentation and client generation
- FastAPI generates OpenAPI specs automatically
- JSON is lightweight and widely supported
- URL versioning is explicit and cache-friendly
- Healthcare integrators familiar with REST patterns
- Easy to test with standard HTTP tools

### Negative
- REST can be verbose for complex operations (multiple round trips)
- Over-fetching/under-fetching compared to GraphQL
- Versioning requires maintaining multiple API versions during transitions

### Risks
- API design decisions are hard to change once clients depend on them
- Version proliferation if not managed carefully

## Alternatives Considered

### GraphQL
- **Pros:** Flexible queries, single endpoint, no over-fetching, strong typing
- **Cons:** Complexity, caching challenges, learning curve, security considerations (query depth)
- **Rejected because:** REST's simplicity and healthcare industry familiarity outweigh GraphQL benefits for this use case

### gRPC
- **Pros:** High performance, strong typing, streaming support, code generation
- **Cons:** Browser support requires proxy, less human-readable, steeper learning curve
- **Rejected because:** Web frontend requirement favors REST; gRPC better for service-to-service

### SOAP
- **Pros:** Strong typing (WSDL), enterprise tooling, WS-Security
- **Cons:** Verbose XML, complex, declining adoption
- **Rejected because:** Modern healthcare APIs favor REST; SOAP adds unnecessary complexity

### JSON-RPC
- **Pros:** Simple, lightweight, language-agnostic
- **Cons:** No standard for documentation, less tooling, not resource-oriented
- **Rejected because:** REST provides better structure and tooling for complex domain

### HL7 FHIR (as primary API)
- **Pros:** Healthcare standard, interoperability, rich data model
- **Cons:** Complex for internal use, overkill for MVP, learning curve
- **Rejected because:** FHIR better suited for external healthcare integrations (future); internal APIs should be simpler. FHIR adapter can be added later for EMR integration.
