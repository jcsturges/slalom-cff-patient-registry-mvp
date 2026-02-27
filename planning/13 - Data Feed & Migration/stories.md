# Epic 13 — Data Feed & Migration

**SRS References:** Sections 10, 11

## Overview

Data Feed & Migration covers two critical data movement capabilities: (1) an ongoing automated nightly data feed from the NGR to the CFF Data Warehouse, and (2) a one-time (re-runnable) historical data migration from the CFF Data Warehouse into all NGR environments. Both capabilities must meet stringent data quality requirements (>=99.9% accuracy), support full and incremental modes, and operate with encrypted, authenticated transmission channels.

## Stories

| Story ID | Title | Priority | SRS Ref |
|----------|-------|----------|---------|
| [13-001](stories/13-001-nightly-delta-feed.md) | Nightly Delta Feed | P0 | 10 |
| [13-002](stories/13-002-deletion-synchronization.md) | Deletion Synchronization | P0 | 10 |
| [13-003](stories/13-003-full-data-resync.md) | Full Data Resynchronization | P1 | 10 |
| [13-004](stories/13-004-feed-reconciliation.md) | Feed Reconciliation Metadata | P0 | 10 |
| [13-005](stories/13-005-feed-security.md) | Feed Security & Transmission | P0 | 10 |
| [13-006](stories/13-006-historical-data-migration.md) | Historical Data Migration | P0 | 11 |
| [13-007](stories/13-007-file-migration.md) | File Migration from portCF | P1 | 11 |
| [13-008](stories/13-008-migration-validation.md) | Migration Validation & Integrity | P0 | 11 |

## Dependencies

- **Epic 06 — Case Report Forms Engine:** Form data model must be stable before migration mapping can be finalized.
- **Epic 05 — Patient Dashboard & Record Management:** Patient identity model must be stable for CFF ID mapping.
- **CFF Data Warehouse:** CFF must provide access credentials, schema documentation, and sample data for both inbound (migration) and outbound (feed) directions.
- **CFF Naming Schema:** Field naming conventions must be provided by CFF for data model alignment.
- **HIPAA-Compliant Storage:** Azure Blob Storage with encryption at rest must be provisioned for file migration target.
- **Epic 12 — Audit Logging & Analytics:** Migration runs must be logged in the audit trail.

## Architecture Notes

### Nightly Delta Feed

```
Azure Functions Timer Trigger (2:00 AM ET)
        |
        v
  Delta Extraction Service
  - Query entities WHERE last_updated_datetime > last_successful_run
  - Query entities WHERE deleted_datetime > last_successful_run
  - Query entities WHERE inactivated_datetime > last_successful_run
        |
        v
  Transform to CFF Schema (field name mapping)
        |
        v
  Write to staging (Azure Blob / SFTP)
        |
        v
  Generate Reconciliation Report
        |
        v
  Update FeedRunLog (status, counts, errors)
```

### Data Model Alignment

The feed service maintains a `FieldMappingConfiguration` table that maps NGR entity properties to CFF Data Warehouse column names. This enables:
- Schema evolution without code changes (mapping is config-driven).
- Consistent field naming between delta and full-refresh modes.
- Auditable mapping history (versioned rows).

### Historical Migration

Migration is executed as a multi-phase process:
1. **Schema Mapping:** Map CFF Data Warehouse tables/columns to NGR entities.
2. **Shared Forms:** Demographics, Diagnosis, Sweat Test, ALD Initiation, Transplant — all current and past patients.
3. **Program-Specific Forms:** Annual Reviews, Encounters, Labs, Care Episodes, Phone Notes — past 10 years.
4. **File Migration:** All portCF uploaded files to Azure Blob Storage (HIPAA-compliant, encryption at rest).
5. **Validation:** Row counts, referential integrity, field-level spot checks, checksum verification.

Each phase is independently re-runnable. A `MigrationRun` table tracks:
- Run ID, phase, timestamp, record counts (source vs target), error count, status.
- Dedicated migration user account (`migration-service@cff.org`) for attribution.
- `IsMigrated` flag on migrated records to distinguish from natively entered data.

### File Migration

Files from portCF are migrated to Azure Blob Storage with:
- Original metadata preserved (filename, upload date, uploader, patient association).
- Patients with reported date of death are excluded from file uploads (per SRS Section 11).
- Content hash (SHA-256) computed at source and verified at destination.
- Container structure: `{environment}/files/{cff-id}/{file-category}/{filename}`.

### Security

All data transmission uses:
- TLS 1.2+ for in-transit encryption.
- Azure Managed Identity or service principal authentication.
- CFF-approved mechanism (SFTP with SSH keys or Azure Blob with SAS tokens — to be confirmed with CFF).
- All credentials stored in Azure Key Vault, never in application configuration.
