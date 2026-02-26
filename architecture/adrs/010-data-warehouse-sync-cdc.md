# ADR-010: Data Warehouse Synchronization - Change Data Capture

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR must synchronize data with the Foundation's existing data warehouse for downstream reporting (CFSmartReports) and analytics. The synchronization must:
- Support nightly batch updates
- Handle inserts, updates, deletes, and merges
- Minimize impact on production database performance
- Provide full and incremental load capabilities
- Align field names with existing data warehouse schema
- Support data validation and reconciliation

## Decision

We will implement **Change Data Capture (CDC)** using application-level tracking with a dedicated `change_data_capture` table.

Synchronization approach:
1. **Application-level CDC**: Track changes via database triggers or application events
2. **Nightly batch job**: Celery scheduled task at 2 AM ET
3. **Incremental sync**: Process only records changed since last sync
4. **Full sync**: Weekly full reconciliation for data integrity
5. **Export views**: Database views aligned with data warehouse schema

## Consequences

### Positive
- **Minimal Production Impact**: CDC table queries don't lock production tables
- **Incremental Efficiency**: Only changed records processed nightly
- **Audit Trail**: CDC records provide change history
- **Flexibility**: Application controls what changes are tracked
- **Reconciliation**: Full sync capability ensures data integrity
- **Schema Alignment**: Export views transform to data warehouse format

### Negative
- **Storage Overhead**: CDC table grows with changes
- **Complexity**: Requires careful trigger/event management
- **Latency**: Not real-time (nightly batch)
- **Maintenance**: CDC table requires periodic cleanup

### Mitigations
- Implement CDC record cleanup after successful sync (30-day retention)
- Monitor CDC table size and sync job performance
- Implement alerting for sync failures
- Design idempotent sync operations for retry safety

## Alternatives Considered

### Database Replication (PostgreSQL Logical Replication)
- **Pros**: Real-time, database-native, minimal application changes
- **Cons**: Schema must match, complex transformation, Azure limitations
- **Rejected**: Data warehouse has different schema requiring transformation

### Timestamp-Based Sync (updated_at queries)
- **Pros**: Simple implementation, no additional tables
- **Cons**: Misses deletes, clock skew issues, full table scans
- **Rejected**: Cannot reliably capture deletes and merges

### Event Streaming (Kafka/Event Hubs)
- **Pros**: Real-time, decoupled, scalable
- **Cons**: Infrastructure complexity, overkill for nightly batch
- **Rejected**: Nightly batch requirement doesn't justify streaming complexity

### ETL Tool (Azure Data Factory)
- **Pros**: Managed service, visual pipeline design
- **Cons**: Additional service, cost, less control over transformation logic
- **Rejected**: Application-level CDC provides more control and simpler architecture
