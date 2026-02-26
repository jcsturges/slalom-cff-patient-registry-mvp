# ADR-007: eCRF Data Storage Strategy - PostgreSQL JSONB

**Status:** Accepted  
**Date:** 2024-01-15

## Context

Electronic Case Report Forms (eCRFs) in the NGR system have the following characteristics:
- Multiple form types (demographics, encounters, annual reports)
- Hundreds of fields per form (Annual Report has 100s of fields)
- Forms evolve over time with new fields and validation rules
- Foundation provides machine-readable form specifications
- Need to support conditional logic and field-level validation
- Historical data must be preserved even as forms change

The storage strategy must balance flexibility for form evolution with query performance for reporting.

## Decision

We will store eCRF submission data as **PostgreSQL JSONB** columns, with form schemas stored separately as versioned JSON Schema definitions.

### Implementation Details
- `form_definitions` table stores form metadata
- `form_versions` table stores versioned JSON Schema definitions
- `form_submissions` table stores submitted data as JSONB
- GIN indexes on JSONB columns for query performance
- Application-layer validation against JSON Schema before storage

## Consequences

### Positive
- Form schema changes don't require database migrations
- Historical submissions retain their original structure
- JSONB supports efficient querying with GIN indexes
- JSON Schema provides standardized validation
- Flexible enough to support Foundation's machine-readable specifications
- Single database technology simplifies operations
- PostgreSQL JSONB has mature tooling and query capabilities

### Negative
- Less type safety than traditional relational columns
- Complex queries across form fields require careful index design
- Schema validation happens at application layer, not database
- Reporting queries may be more complex than relational equivalent

### Risks
- JSONB query performance could degrade with complex patterns (mitigated by proper indexing)
- Schema drift between form versions needs careful management
- Large JSONB documents could impact storage efficiency

## Alternatives Considered

### Traditional Relational Tables (EAV Pattern)
- **Pros:** Strong typing, standard SQL queries, referential integrity
- **Cons:** Schema changes require migrations, EAV queries are complex and slow
- **Rejected because:** Form evolution frequency would create migration burden

### MongoDB (Document Database)
- **Pros:** Native document model, flexible schema, horizontal scaling
- **Cons:** Separate database to manage, weaker ACID, eventual consistency concerns
- **Rejected because:** PostgreSQL JSONB provides sufficient flexibility without additional infrastructure

### Separate Relational Tables per Form Type
- **Pros:** Strong typing, optimized queries per form
- **Cons:** Schema changes require migrations, many tables to manage, complex joins
- **Rejected because:** Maintenance burden too high for evolving form requirements

### XML Storage
- **Pros:** Schema validation (XSD), mature tooling
- **Cons:** Verbose, slower parsing, less developer-friendly than JSON
- **Rejected because:** JSON is modern standard, better tooling, smaller storage footprint

### Hybrid (Relational + JSONB)
- **Pros:** Common fields in columns, flexible fields in JSONB
- **Cons:** Complexity of deciding what goes where, migration when fields become common
- **Rejected because:** Added complexity without proportional benefit for this use case
