# ADR-004: Primary Database Selection - PostgreSQL

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires a primary database for storing patient records, form submissions, encounters, and all transactional data. The database must:
- Support ACID transactions for data integrity
- Handle ~50,000 patient records and ~4 million encounters
- Support complex queries for reporting
- Provide JSONB support for flexible form data storage
- Scale to projected growth (~1,000 patients/year, ~120,000 encounters/year)
- Support HIPAA compliance requirements

## Decision

We will use **PostgreSQL 15.5** via **Azure Database for PostgreSQL Flexible Server** as the primary database.

Configuration:
- General Purpose tier with 4 vCores (scalable to 64)
- 512 GB storage with auto-grow
- Zone-redundant high availability
- Point-in-time recovery enabled (35-day retention)
- Read replicas for reporting queries

## Consequences

### Positive
- **JSONB Support**: Native JSONB with indexing enables flexible form data storage
- **Reliability**: Proven reliability for healthcare and enterprise applications
- **SQL Compliance**: Full SQL standard compliance for complex reporting queries
- **Partitioning**: Native table partitioning for efficient historical data management
- **Full-Text Search**: Built-in full-text search for patient name lookups
- **Managed Service**: Azure manages backups, patching, and high availability
- **Cost Effective**: Open-source with no licensing costs
- **Ecosystem**: Excellent tooling, ORMs, and migration tools (SQLAlchemy, Alembic)

### Negative
- **Vertical Scaling Limits**: Eventually requires sharding for extreme scale (not expected)
- **Managed Service Constraints**: Some advanced features limited in managed offering
- **Connection Limits**: Requires connection pooling for high-concurrency scenarios

### Mitigations
- Implement PgBouncer for connection pooling
- Use read replicas to offload reporting queries
- Design partitioning strategy for large tables (encounters, form_submissions)
- Monitor and optimize query performance proactively

## Alternatives Considered

### Microsoft SQL Server
- **Pros**: Excellent Azure integration, strong enterprise features
- **Cons**: Licensing costs, less flexible JSON support, vendor lock-in
- **Rejected**: Licensing costs and PostgreSQL's superior JSONB support

### MySQL/MariaDB
- **Pros**: Wide adoption, good performance, Azure managed option
- **Cons**: Weaker JSON support, less advanced features (partitioning, CTEs)
- **Rejected**: PostgreSQL's advanced features better suited for complex healthcare data

### MongoDB
- **Pros**: Flexible schema, excellent for document storage
- **Cons**: Weaker ACID guarantees, less suitable for relational data, complex transactions
- **Rejected**: Healthcare data requires strong ACID guarantees and relational integrity

### Azure Cosmos DB
- **Pros**: Global distribution, multiple APIs, managed scaling
- **Cons**: Higher cost, eventual consistency challenges, complex pricing model
- **Rejected**: Cost concerns and complexity for primarily relational data model
