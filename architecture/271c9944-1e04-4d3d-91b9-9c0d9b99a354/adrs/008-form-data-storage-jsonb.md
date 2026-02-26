# ADR-008: Form Data Storage Strategy - PostgreSQL JSONB

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR must store electronic Case Report Form (eCRF) submissions with hundreds of fields per form. Forms evolve over time with new fields and validation rules. The storage strategy must support efficient querying for reporting while accommodating schema flexibility.

## Decision

We will store form submission data as **PostgreSQL JSONB** columns with form definitions stored separately as JSON Schema.

## Consequences

### Positive
- Flexible schema accommodates form evolution without database migrations
- JSONB indexing (GIN) enables efficient queries on form fields
- Form versioning handled through separate form_definitions table
- Supports complex nested data structures
- PostgreSQL JSONB operators enable powerful queries
- Maintains relational integrity for patient/organization relationships
- Single database technology simplifies operations

### Negative
- Less strict schema enforcement at database level
- JSONB queries can be less intuitive than column queries
- Reporting queries may be more complex

### Risks
- Data quality issues from flexible schema
- Mitigation: Enforce validation at application layer; use JSON Schema validation before storage

## Alternatives Considered

### Entity-Attribute-Value (EAV) Pattern
- **Pros:** Fully relational, flexible schema
- **Cons:** Complex queries, poor performance, difficult to maintain
- **Rejected because:** EAV queries are notoriously slow and complex for reporting

### Separate Table Per Form Type
- **Pros:** Strong typing, simple queries, good performance
- **Cons:** Schema migrations for every form change, table proliferation
- **Rejected because:** Form evolution would require frequent database migrations

### Document Database (MongoDB)
- **Pros:** Native document storage, flexible schema
- **Cons:** Separate database, weaker relational support, operational complexity
- **Rejected because:** Adds infrastructure complexity; PostgreSQL JSONB provides sufficient document capabilities

### Hybrid (Core Fields + JSONB Extension)
- **Pros:** Common fields indexed, flexibility for extras
- **Cons:** Complexity in determining what's core vs. extension
- **Rejected because:** Form fields vary significantly; most fields would end up in JSONB anyway
