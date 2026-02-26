# ADR-006: API Architecture Pattern

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR API must support complex business operations including:

- Patient roster management (CRUD, transfer, merge)
- Dynamic form handling with validation rules
- Report generation with complex queries
- Data import/export operations
- Audit logging for all operations
- Integration with external systems

The architecture must be maintainable, testable, and support future extensibility for EMR integration, patient portal, and other planned features.

## Decision

We will implement **Clean Architecture** with a **CQRS-lite** pattern for the ASP.NET Core 8 Web API.

**Layer structure:**

```
NGR.Api/
├── Controllers/           # Presentation layer - HTTP endpoints
├── Middleware/           # Cross-cutting concerns
└── Program.cs

NGR.Application/
├── Commands/             # Write operations (CQRS)
├── Queries/              # Read operations (CQRS)
├── Services/             # Application services
├── Validators/           # FluentValidation validators
├── Mappings/             # AutoMapper profiles
└── Interfaces/           # Port definitions

NGR.Domain/
├── Entities/             # Domain entities
├── ValueObjects/         # Value objects
├── Enums/                # Domain enumerations
├── Events/               # Domain events
└── Exceptions/           # Domain exceptions

NGR.Infrastructure/
├── Persistence/          # EF Core DbContext, repositories
├── Identity/             # Okta integration
├── Storage/              # Blob storage
├── Monitoring/           # App Insights
└── ExternalServices/     # Data warehouse connector
```

**Key patterns:**
- **Repository pattern**: Abstract data access behind interfaces
- **Specification pattern**: Encapsulate complex query logic
- **MediatR**: Optional for command/query dispatch (evaluate complexity)
- **Result pattern**: Explicit success/failure handling without exceptions

## Consequences

### Positive
- **Separation of concerns**: Clear boundaries between layers
- **Testability**: Each layer can be tested independently
- **Maintainability**: Changes isolated to specific layers
- **Extensibility**: New features added without modifying existing code
- **CQRS benefits**: Optimized read and write paths
- **Domain focus**: Business logic centralized in domain layer

### Negative
- **Initial complexity**: More files and abstractions than simple CRUD
- **Learning curve**: Team must understand Clean Architecture principles
- **Overhead**: Simple operations may feel over-engineered

### Risks
- **Over-abstraction**: Must avoid unnecessary interfaces and layers
- **Performance**: Additional layers add minimal overhead (acceptable)
- **Consistency**: Team must follow patterns consistently

## Alternatives Considered

### Traditional N-Tier Architecture
- **Rejected**: Less testable; business logic often leaks into controllers

### Vertical Slice Architecture
- **Rejected**: Better for larger teams; Clean Architecture more familiar

### Minimal API (no architecture)
- **Rejected**: Not maintainable for complex business logic; harder to test
