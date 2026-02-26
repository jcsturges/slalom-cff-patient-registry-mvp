# ADR-003: Primary Database Selection - PostgreSQL

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires a primary database to store:
- Patient records (~50,000+ patients)
- Encounter data (~4 million records, growing 120,000/year)
- Electronic Case Report Forms with flexible schemas
- User and access control data
- Audit logs for HIPAA compliance

The database must support HIPAA compliance, handle both structured and semi-structured data (form submissions), and provide strong consistency guarantees for healthcare data.

## Decision

We will use **PostgreSQL 15.x** via **Azure Database for PostgreSQL Flexible Server** as the primary database.

## Consequences

### Positive
- JSONB support enables flexible form data storage without schema migrations per form change
- Strong ACID compliance critical for healthcare data integrity
- Mature, battle-tested with 25+ years of development
- Excellent Azure managed service with automatic backups, PITR, and HA
- Native full-text search reduces need for separate search infrastructure
- Table partitioning for audit log management
- Read replicas for reporting workload isolation
- Transparent Data Encryption (TDE) for HIPAA compliance
- Rich indexing options (B-tree, GIN, GiST) for query optimization

### Negative
- Horizontal scaling more complex than NoSQL alternatives
- JSONB queries can be slower than dedicated document stores for complex patterns
- Requires connection pooling (PgBouncer) for high connection counts

### Risks
- Single database could become bottleneck (mitigated by read replicas and caching)
- JSONB schema evolution requires application-level management

## Alternatives Considered

### Microsoft SQL Server
- **Pros:** Microsoft ecosystem alignment, excellent tooling, JSON support
- **Cons:** Higher licensing costs, less flexible JSON handling, vendor lock-in
- **Rejected because:** PostgreSQL provides equivalent functionality at lower cost with better JSON support

### MongoDB
- **Pros:** Native document model ideal for forms, horizontal scaling, flexible schema
- **Cons:** Weaker ACID guarantees, eventual consistency concerns for healthcare, less mature Azure offering
- **Rejected because:** ACID compliance and relational integrity more important than document flexibility

### Amazon Aurora PostgreSQL
- **Pros:** PostgreSQL compatible, excellent performance, auto-scaling storage
- **Cons:** AWS-only, would require cross-cloud architecture
- **Rejected because:** Azure alignment requirement from ADR-001

### CockroachDB
- **Pros:** PostgreSQL compatible, distributed by design, strong consistency
- **Cons:** Higher operational complexity, less mature, higher cost
- **Rejected because:** Distributed database complexity not justified for current scale requirements

### MySQL
- **Pros:** Widely used, good performance, Azure managed service available
- **Cons:** Weaker JSON support, less advanced features, licensing concerns (Oracle)
- **Rejected because:** PostgreSQL's JSONB and advanced features better suited for eCRF storage
