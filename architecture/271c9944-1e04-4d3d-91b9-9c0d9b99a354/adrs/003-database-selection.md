# ADR-003: Database Selection

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires a relational database to store patient data, encounter records, form definitions, user information, and audit logs. The database must support:

- ~35,000 active patients with growth of ~1,000/year
- ~4 million encounters with growth of ~120,000/year
- Estimated 1 TB data volume
- HIPAA compliance (encryption, audit logging)
- High availability and disaster recovery
- Integration with Entity Framework Core
- Complex queries for dynamic reporting

## Decision

We will use **Azure SQL Database** (General Purpose tier, Gen5, 8 vCores) as the primary data store.

**Configuration:**
- Service tier: General Purpose
- Compute: Gen5, 8 vCores (Production)
- Storage: 1 TB with auto-grow
- Backup: Geo-redundant backup storage
- High availability: Zone redundant (Production)
- Security: Transparent Data Encryption (TDE), Azure AD authentication

**Entity Framework Core 8.0** will be used as the ORM with the following patterns:
- Code-first migrations
- Repository pattern for data access abstraction
- Specification pattern for complex queries

## Consequences

### Positive
- **Mandated by tech stack**: Aligns with Foundation's required technology stack
- **Managed service**: No infrastructure management overhead
- **HIPAA compliance**: TDE, audit logging, and Azure compliance certifications
- **High availability**: Built-in HA with zone redundancy option
- **Disaster recovery**: Point-in-time restore (35 days) and geo-restore
- **Scalability**: Easy vertical scaling; read replicas for reporting
- **EF Core integration**: Excellent support with Microsoft.EntityFrameworkCore.SqlServer
- **Familiar SQL**: T-SQL syntax familiar to most database developers
- **Azure integration**: Native integration with Key Vault, App Service, and monitoring

### Negative
- **Cost**: Higher cost than open-source alternatives at scale
- **Vendor lock-in**: Azure-specific features may complicate future migration
- **DTU/vCore complexity**: Sizing requires understanding of Azure SQL pricing models

### Risks
- **Performance tuning**: May require index optimization for complex reporting queries
- **Connection limits**: Must implement connection pooling and retry logic
- **Cost growth**: Storage and compute costs scale with data volume

## Alternatives Considered

### PostgreSQL (Azure Database for PostgreSQL)
- **Rejected**: Not in mandated tech stack; would require different EF Core provider

### SQL Server on VM
- **Rejected**: Higher operational overhead; less integrated with Azure PaaS features

### Cosmos DB
- **Rejected**: Not suitable for relational data model; overkill for current scale
