# Epic 12 — Audit Logging & Analytics

**SRS References:** Sections 1.8, 3.8.8

## Overview

Audit Logging & Analytics provides the foundational observability layer for the entire NGR platform. Every significant user and system action is captured in a tamper-evident audit log that excludes PHI/PII content while retaining opaque identifiers for traceability. This epic also covers system monitoring, error handling standards, and the analytics data pipeline that feeds Foundation Admin reporting (Epic 11).

## Stories

| Story ID | Title | Priority | SRS Ref |
|----------|-------|----------|---------|
| [12-001](stories/12-001-application-audit-logging.md) | Application Audit Logging | P0 | 1.8.1 |
| [12-002](stories/12-002-patient-record-change-tracking.md) | Patient Record Change Tracking | P0 | 3.8.8 |
| [12-003](stories/12-003-report-usage-tracking.md) | Report Usage Tracking | P1 | 3.8.8 |
| [12-004](stories/12-004-user-interaction-analytics.md) | User Interaction Analytics | P1 | 3.8.8 |
| [12-005](stories/12-005-analytics-data-export.md) | Analytics Data Export | P1 | 3.8.8 |
| [12-006](stories/12-006-monitoring-dashboard.md) | Monitoring Dashboard | P1 | 1.8.2 |
| [12-007](stories/12-007-user-facing-error-handling.md) | User-Facing Error Handling | P0 | 1.8.3 |

## Dependencies

- **Epic 01 — Authentication & User Management:** User identity and role claims must be available in the request context for audit attribution.
- **Epic 06 — Case Report Forms Engine:** Form save/submit events must emit structured change events for field-level tracking.
- **Epic 08 — Reporting & Report Builder:** Report execution must emit usage events for tracking.
- **Azure Application Insights:** Must be provisioned (Epic infrastructure / IaC) for system monitoring metrics.

## Architecture Notes

### Audit Log Storage

Audit events are stored in a dedicated `AuditLog` table in Azure SQL with the following design principles:
- **Append-only:** No UPDATE or DELETE operations on audit rows. Retention policy handled by automated archival jobs.
- **PHI-free:** All logged values use opaque identifiers (CFF ID, internal GUIDs). Field-level change values for PHI fields are replaced with `[REDACTED]`.
- **Indexed:** Composite indexes on (UserId, Timestamp), (PatientId, Timestamp), and (EventType, Timestamp) for efficient querying.
- **Partitioned:** Monthly partitioning by event timestamp for performance at scale.

### Event Pipeline

```
Controller / Service Action
        |
        v
  AuditInterceptor (EF Core SaveChanges override)
        |
        v
  AuditEvent DTO (structured, PHI-stripped)
        |
        v
  AuditLog Table (Azure SQL)
        |
        v
  Analytics Query Layer (Foundation Admin Tools, Epic 11)
        |
        v
  Scheduled Export (Azure Functions timer -> SFTP/Blob)
```

### Change Tracking Implementation

Field-level change detection uses EF Core's `ChangeTracker` API:
- On `SaveChanges`, the interceptor inspects `EntityEntry.State` (Added / Modified / Deleted).
- For Modified entities, `CurrentValues` vs `OriginalValues` are compared property-by-property.
- Properties decorated with `[SensitiveData]` attribute have their values replaced with `[REDACTED]` in the audit record.
- The change record includes: entity type, entity ID, property name, old value, new value, change type.

### Monitoring

Application Insights provides the primary monitoring layer:
- Custom metrics for request latency, error rates, dependency health.
- Availability tests for `/health` endpoint.
- Alert rules for error rate spikes, latency degradation, and availability drops.
- A Grafana or Azure Dashboard surfaces these metrics for CFF operations staff.

### Error Handling

All user-facing errors follow a structured pattern:
- HTTP responses use RFC 7807 Problem Details format.
- Error messages are non-technical and actionable (e.g., "Unable to save the form. Please try again or contact support.").
- Internal exception details, stack traces, and any PHI are logged server-side only, never returned to the client.
- The React SPA uses a global error boundary with user-friendly fallback UI.
