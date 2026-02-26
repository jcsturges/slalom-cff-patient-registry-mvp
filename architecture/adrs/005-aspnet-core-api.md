# ADR-005: ASP.NET Core 8 Web API for Backend

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires a robust backend API to:
- Handle CRUD operations for patients, encounters, and forms
- Enforce complex business rules for form ownership and editing
- Process CSV imports with validation
- Generate reports with dynamic queries
- Integrate with the Foundation's data warehouse
- Support 3,000+ users across 136 care centers

Requirements include:
- High performance for data-intensive operations
- Strong typing for complex domain models
- Comprehensive audit logging
- Integration with Azure services
- Minimum 70% unit test coverage

The RFP tech stack mandates ASP.NET Core 8 Web API (C# 12).

## Decision

We will use **ASP.NET Core 8 Web API** with the following architecture:

### Architecture Pattern
- **Clean Architecture** with layers:
  - API (Controllers, Middleware)
  - Application (Services, DTOs, Validators)
  - Domain (Entities, Value Objects, Interfaces)
  - Infrastructure (EF Core, External Services)

### Key Libraries
- **Entity Framework Core 8**: ORM with Azure SQL
- **FluentValidation 11.9.x**: Request validation
- **AutoMapper 13.0.x**: Object mapping
- **Serilog 8.0.x**: Structured logging
- **Swashbuckle 6.5.x**: OpenAPI documentation

### API Design
- RESTful endpoints with consistent naming
- API versioning via URL path (/api/v1/)
- Pagination for list endpoints
- Problem Details (RFC 7807) for errors

## Consequences

### Positive
- **Performance**: ASP.NET Core is one of the fastest web frameworks
- **Type Safety**: C# 12 provides strong typing and null safety
- **Azure Integration**: First-class support for Azure services
- **Mature Ecosystem**: Extensive NuGet packages for enterprise scenarios
- **Long-Term Support**: .NET 8 is LTS (supported until Nov 2026)
- **Testability**: Built-in DI container enables easy unit testing

### Negative
- **Memory Usage**: Higher memory footprint than some alternatives
- **Cold Start**: Longer startup time than lightweight frameworks
- **Complexity**: Clean Architecture adds initial development overhead
- **Windows Heritage**: Some Azure features work better on Windows

### Mitigations
- Use Linux containers for better resource efficiency
- Enable "Always On" in App Service to prevent cold starts
- Create project templates to reduce boilerplate
- Document architecture patterns for team consistency

## Alternatives Considered

### Node.js (Express/NestJS)
- **Rejected**: Not in approved tech stack
- Would provide faster cold starts
- Less type safety without TypeScript discipline

### Java (Spring Boot)
- **Rejected**: Not in approved tech stack
- Similar enterprise capabilities
- Different skill set required

### Python (FastAPI)
- **Rejected**: Not in approved tech stack
- Good for rapid development
- Less suitable for complex domain models
