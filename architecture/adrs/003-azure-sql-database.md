# ADR-003: Azure SQL Database for Data Persistence

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires a relational database to store:
- ~50,000 patient records (growing ~1,000/year)
- ~4 million encounter records (growing ~120,000/year)
- Form definitions and submissions with JSON data
- User, role, and permission data
- Audit logs for compliance

Requirements include:
- HIPAA compliance for PHI storage
- High availability (99.99%+ for production)
- Encryption at rest and in transit
- Support for complex queries and reporting
- Integration with Entity Framework Core

The RFP tech stack mandates Azure SQL Database (NOT PostgreSQL).

## Decision

We will use **Azure SQL Database** with the following configuration:

1. **Service Tier**: Business Critical
   - Provides 99.99% SLA with zone redundancy
   - Built-in read replica for reporting workloads
   - Local SSD storage for low latency

2. **Compute**: Provisioned, 8 vCores (scalable to 16)
   - Predictable performance for consistent workloads
   - Can scale up during peak periods

3. **Storage**: 1 TB with auto-grow enabled
   - Sufficient for current data + 5 years growth
   - Geo-redundant backups with 35-day retention

4. **Security**:
   - Transparent Data Encryption (TDE) enabled
   - Azure AD authentication via Managed Identity
   - Private endpoint for network isolation
   - Advanced Threat Protection enabled

5. **ORM**: Entity Framework Core 8 with code-first migrations

## Consequences

### Positive
- **HIPAA Compliant**: Azure SQL is HIPAA BAA eligible with TDE
- **High Availability**: Business Critical tier provides 99.99% SLA
- **Read Scale-out**: Built-in read replica for reporting queries
- **Managed Service**: Automatic patching, backups, and monitoring
- **EF Core Support**: First-class support for Entity Framework Core
- **JSON Support**: Native JSON functions for form data queries
- **Geo-Replication**: Easy DR setup with geo-secondary

### Negative
- **Cost**: Business Critical tier is more expensive than General Purpose
- **Vendor Lock-in**: T-SQL specific features limit portability
- **Connection Limits**: Must manage connection pooling carefully
- **Size Limits**: 4 TB max for Business Critical (sufficient for NGR)

### Mitigations
- Use connection pooling in Entity Framework Core
- Implement retry logic with exponential backoff
- Monitor DTU/vCore usage and scale proactively
- Use read replica for reporting to reduce primary load

## Alternatives Considered

### PostgreSQL (Azure Database for PostgreSQL)
- **Rejected**: Explicitly excluded by RFP tech stack
- Would provide similar capabilities
- Better JSON support but less .NET ecosystem integration

### Azure Cosmos DB
- **Rejected**: Not suitable for relational data model
- Would require significant schema redesign
- Overkill for current scale requirements

### SQL Server on Azure VM
- **Rejected**: Too much operational overhead
- Requires manual HA configuration
- Higher total cost of ownership
