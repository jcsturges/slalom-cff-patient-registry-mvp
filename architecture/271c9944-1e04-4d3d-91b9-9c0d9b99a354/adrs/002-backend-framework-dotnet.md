# ADR-002: Backend Framework Selection - .NET 8.0 with ASP.NET Core

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR backend requires a robust, performant framework capable of handling healthcare data with strong security features. The framework must support long-term maintenance (5+ years), have excellent Azure integration, and provide strong typing for complex domain models.

## Decision

We will use **.NET 8.0 LTS with ASP.NET Core** for all backend services.

## Consequences

### Positive
- Long-term support (LTS) until November 2026, with extended support options
- Excellent Azure integration (native SDKs, managed identity, App Service/AKS optimization)
- Strong typing with C# reduces runtime errors in healthcare-critical code
- High performance - consistently ranks among fastest web frameworks
- Mature ecosystem with extensive healthcare/enterprise libraries
- Built-in dependency injection, middleware pipeline, and configuration management
- Entity Framework Core provides robust ORM with PostgreSQL support
- Comprehensive security features (authentication, authorization, data protection)

### Negative
- Smaller talent pool compared to Node.js/Python in some markets
- Heavier runtime compared to Go or Rust
- Microsoft-centric ecosystem may limit some open-source options

### Risks
- .NET developer availability in Foundation's geographic area
- Mitigation: .NET is widely used in healthcare; remote work options available

## Alternatives Considered

### Node.js with Express/NestJS
- **Pros:** Large developer pool, fast development, JavaScript full-stack
- **Cons:** Weaker typing (even with TypeScript), less mature enterprise patterns, callback complexity
- **Rejected because:** Healthcare applications benefit from strong typing; .NET's enterprise patterns better suit complex domain logic

### Python with FastAPI
- **Pros:** Rapid development, excellent for data science integration, readable code
- **Cons:** Performance limitations at scale, GIL constraints, less mature async patterns
- **Rejected because:** Performance concerns for high-throughput form submissions; typing is optional rather than enforced

### Java with Spring Boot
- **Pros:** Mature enterprise framework, large talent pool, strong typing
- **Cons:** Verbose syntax, slower startup times, heavier memory footprint
- **Rejected because:** Similar capabilities to .NET but without the Azure-native integration benefits

### Go
- **Pros:** Excellent performance, simple deployment, strong concurrency
- **Cons:** Less mature ORM options, smaller ecosystem for enterprise patterns, steeper learning curve
- **Rejected because:** Limited ORM maturity for complex healthcare data models; smaller ecosystem for enterprise features
