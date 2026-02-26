# ADR-002: Backend Framework Selection

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR backend must provide a secure, scalable REST API to support patient registry operations, form management, reporting, and integration with external systems (Okta, Data Warehouse). The API must handle sensitive PHI data with appropriate security controls and support HIPAA compliance requirements.

Key requirements:
- RESTful API design
- Integration with Okta for JWT validation
- Entity Framework Core for Azure SQL Database access
- Azure App Service hosting compatibility
- Strong typing and compile-time safety
- Comprehensive testing support
- Long-term support and security updates

## Decision

We will use **ASP.NET Core 8 Web API with C# 12** as the backend framework.

**Specific versions and key packages:**
- ASP.NET Core 8.0.0
- C# 12.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- Okta.AspNetCore 4.5.0
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- Azure.Security.KeyVault.Secrets 4.5.0
- Azure.Storage.Blobs 12.19.1
- Microsoft.ApplicationInsights.AspNetCore 2.22.0
- FluentValidation 11.9.0
- AutoMapper 12.0.1
- Swashbuckle.AspNetCore 6.5.0

**Architecture pattern:** Clean Architecture with CQRS-lite pattern for complex operations.

## Consequences

### Positive
- **Mandated by tech stack**: Aligns with Foundation's required technology stack
- **LTS support**: .NET 8 is a Long-Term Support release (supported until November 2026)
- **Performance**: ASP.NET Core 8 offers excellent performance benchmarks
- **Azure integration**: First-class support for Azure services (Key Vault, App Insights, Blob Storage)
- **Okta SDK**: Official Okta.AspNetCore package provides robust authentication middleware
- **Entity Framework Core 8**: Mature ORM with excellent Azure SQL support
- **Type safety**: C# 12 provides strong typing and modern language features
- **Testing**: xUnit, Moq, and FluentAssertions provide comprehensive testing capabilities

### Negative
- **Windows hosting preference**: While cross-platform, some Azure features work better on Windows App Service
- **Memory footprint**: Higher baseline memory usage compared to lightweight frameworks
- **Cold start**: Initial request latency on App Service; mitigated by Always On setting

### Risks
- **Package vulnerabilities**: Must monitor NuGet packages for security updates
- **Breaking changes**: Major version updates may require code changes

## Alternatives Considered

### Node.js/Express
- **Rejected**: Not in mandated tech stack; less mature enterprise patterns; weaker typing

### Java/Spring Boot
- **Rejected**: Not in mandated tech stack; different ecosystem from frontend

### .NET 6
- **Rejected**: .NET 8 offers better performance and longer support window
