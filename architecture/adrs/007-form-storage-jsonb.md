# ADR-007: Form Data Storage Strategy - JSONB with Denormalized Fields

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR must store electronic Case Report Form (eCRF) submissions with hundreds of fields per form. Forms evolve over time with new fields and validation rules. The storage strategy must:
- Support flexible schema evolution without database migrations
- Enable efficient querying for reporting
- Maintain data integrity and validation
- Support form versioning
- Allow field-level access control for PHI

## Decision

We will use a **hybrid approach** combining:
1. **JSONB column** (`form_data`) in `form_submissions` table for complete form storage
2. **Denormalized `field_values` table** for frequently queried fields
3. **Form definitions** stored as JSON Schema in `form_definitions` table

Storage pattern:
```sql
-- Primary storage in JSONB
form_submissions.form_data = {
  "patient_weight": 70.5,
  "fev1_percent": 85,
  "medication_list": ["drug_a", "drug_b"],
  ...
}

-- Denormalized for queries
field_values (form_submission_id, field_code, value_text, value_number, value_date, ...)
```

## Consequences

### Positive
- **Schema Flexibility**: New form fields don't require database migrations
- **Version Support**: Different form versions can coexist with different schemas
- **Query Performance**: Denormalized fields enable efficient SQL queries for reporting
- **Complete Data**: JSONB preserves complete submission for audit and reconstruction
- **Validation**: JSON Schema enables consistent validation across frontend and backend
- **GIN Indexing**: PostgreSQL GIN indexes enable efficient JSONB queries

### Negative
- **Data Duplication**: Field values stored in both JSONB and denormalized table
- **Sync Complexity**: Must keep JSONB and denormalized values in sync
- **Storage Overhead**: Duplication increases storage requirements
- **Query Complexity**: Some queries require joining JSONB and relational data

### Mitigations
- Use database triggers or application logic to maintain sync
- Only denormalize fields needed for reporting (not all fields)
- Implement validation to ensure JSONB and denormalized values match
- Create materialized views for complex reporting queries

## Alternatives Considered

### Fully Normalized (EAV Pattern)
- **Pros**: No duplication, flexible schema
- **Cons**: Complex queries, poor performance for full form retrieval, many joins
- **Rejected**: Query complexity and performance concerns for hundreds of fields

### Fully Denormalized (Wide Tables)
- **Pros**: Simple queries, good performance
- **Cons**: Schema changes require migrations, sparse columns, table width limits
- **Rejected**: Form evolution would require frequent schema migrations

### Document Database (MongoDB)
- **Pros**: Native document storage, flexible schema
- **Cons**: Separate database, weaker ACID, complex joins with relational data
- **Rejected**: PostgreSQL JSONB provides sufficient document capabilities

### Separate Columns per Field
- **Pros**: Strong typing, simple queries
- **Cons**: Hundreds of columns, frequent migrations, maintenance burden
- **Rejected**: Not practical for forms with hundreds of evolving fields
