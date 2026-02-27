# Epic 11 — Foundation Admin Tools

**SRS References:** Sections 3.8.7, 3.8.8, 5.2.3, 6.2.3

## Overview

Foundation Admin Tools provide privileged CFF staff with administrative capabilities that span the entire registry: annual database lock management, user impersonation for support and auditing, and view-level analytics on user activity and patient record changes. These tools are restricted to the **FoundationAdmin** role and carry heightened audit requirements due to their elevated access.

## Stories

| Story ID | Title | Priority | SRS Ref |
|----------|-------|----------|---------|
| [11-001](stories/11-001-database-lock-ui.md) | Database Lock UI | P0 | 3.8.7, 6.2.3 |
| [11-002](stories/11-002-database-lock-execution.md) | Database Lock Execution Engine | P0 | 3.8.7, 6.2.3 |
| [11-003](stories/11-003-user-impersonation.md) | User Impersonation Activation | P0 | 5.2.3 |
| [11-004](stories/11-004-impersonation-session-ux.md) | Impersonation Session UX | P0 | 5.2.3 |
| [11-005](stories/11-005-impersonation-audit-trail.md) | Impersonation Audit Trail | P0 | 5.2.3 |

## Dependencies

- **Epic 01 — Authentication & User Management:** Okta RBAC with FoundationAdmin role must be in place.
- **Epic 06 — Case Report Forms Engine:** Form engine must support lock-state awareness for date validation and read-only rendering.
- **Epic 12 — Audit Logging & Analytics:** Audit infrastructure must be available for impersonation tracking and analytics views.
- **Epic 04 — Program Roster & Patient Identity:** User Search Interface must exist for impersonation entry point.
- **Epic 02 — Care Program Management:** Program user list must exist for impersonation entry point.

## Architecture Notes

### Database Lock

The database lock operates at two levels:
1. **Metadata level:** A `DatabaseLock` record stores the reporting year, lock date, execution mode, and status (Pending / InProgress / Completed / Failed).
2. **Form level:** Individual form submissions are flagged as locked via a `IsLocked` boolean and `LockedAt` timestamp. Post-lock, the form engine enforces date validation rules restricting new entries to the current (non-locked) year.

The lock engine must handle two execution modes:
- **Fast Synchronous** (less than 2 min): Processes all eligible forms in a single transaction. Suitable when volume is manageable.
- **Overnight Batch** (2-6 AM ET): Queued via Hangfire or Azure Functions timer trigger. Processes forms in batches with progress tracking.

### User Impersonation

Impersonation is implemented as a session-mode overlay, not a role mutation:
- The API issues a scoped "impersonation token" that carries both the acting admin's identity and the target user's effective permissions.
- All downstream authorization checks use the target user's roles/claims.
- Audit middleware extracts both identities from the token for every logged action.
- The impersonation session is time-bounded (configurable, default 60 minutes) with explicit exit.

### Analytics Views

Analytics data is read from the audit log tables (Epic 12). Foundation Admin Tools provide the query UI and export capabilities; the underlying data collection is owned by Epic 12.
