# ADR-002: Backend Framework Selection - Python FastAPI

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR backend requires a framework to build RESTful APIs for all system functionality. The framework must support:
- High performance for concurrent users (~3,000 care center staff)
- Strong typing and validation for healthcare data
- Automatic API documentation for developer productivity
- Async capabilities for I/O-bound operations
- Mature ecosystem for healthcare/data processing libraries

## Decision

We will use **Python 3.11 with FastAPI 0.109.x** as the backend framework for all API services.

## Consequences

### Positive
- Automatic OpenAPI 3.0 documentation generation reduces documentation burden
- Pydantic integration provides robust request/response validation
- Native async/await support for high concurrency without threading complexity
- Python ecosystem has excellent healthcare libraries (HL7, FHIR parsers for future)
- Strong data processing capabilities (pandas, numpy) for reporting features
- Type hints improve code quality and IDE support
- Performance comparable to Node.js/Go for I/O-bound workloads
- Large talent pool for hiring and maintenance

### Negative
- Python GIL limits CPU-bound parallelism (mitigated by horizontal scaling)
- Slower than compiled languages for compute-intensive operations
- Runtime type checking vs compile-time (mitigated by mypy in CI)

### Risks
- FastAPI is younger than Django/Flask (mitigated by strong community adoption)
- Async programming requires team familiarity

## Alternatives Considered

### Django REST Framework (Python)
- **Pros:** Battle-tested, extensive ecosystem, built-in admin, ORM included
- **Cons:** Synchronous by default, heavier framework, slower performance, more opinionated
- **Rejected because:** Async support is bolted-on, performance overhead for API-focused application

### Node.js with Express/NestJS
- **Pros:** Excellent async performance, large ecosystem, TypeScript support
- **Cons:** Less mature data processing libraries, callback complexity, weaker typing than Python+Pydantic
- **Rejected because:** Python's data ecosystem better suited for healthcare reporting requirements

### Go with Gin/Echo
- **Pros:** Excellent performance, compiled binaries, strong concurrency
- **Cons:** Smaller ecosystem, less expressive for complex business logic, steeper learning curve
- **Rejected because:** Development velocity more important than raw performance for this workload

### Java with Spring Boot
- **Pros:** Enterprise-proven, excellent tooling, strong typing
- **Cons:** Verbose, slower development velocity, heavier resource footprint
- **Rejected because:** Overhead not justified for team size and project scope

### .NET Core with ASP.NET
- **Pros:** Microsoft ecosystem alignment, excellent performance, strong typing
- **Cons:** Smaller open-source ecosystem, less data science tooling
- **Rejected because:** Python's healthcare and data libraries provide more value for NGR requirements
