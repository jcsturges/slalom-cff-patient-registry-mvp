# Epic 02: Care Program Management

## Overview

This epic covers the data model, administration, and lifecycle management for CF Care Programs — the organizational units that connect patients, users, and clinical data within the Registry. Each care program represents an accredited cystic fibrosis care center with a unique, immutable Program ID.

Foundation Administrators are the sole managers of care program records. They can create, edit, and deactivate programs but can never delete them or change their IDs. The system also requires two special program types: the Orphaned Record Holding (ORH) program (a system-reserved administrative program that safeguards patient records with no active clinical program association) and Training Programs (which mirror full program functionality but are deterministically excluded from all analytics, reporting, and downstream data feeds).

## Outcomes

- Care program data model supports all required metadata fields, flags, and calculated display title
- Foundation Administrators can create, search, view, edit, and deactivate care programs
- Program ID immutability and uniqueness enforced at all levels
- ORH program (ID 3000) auto-associates orphaned patients and is hidden from care program users
- Training programs provide full workflow equivalence while being excluded from analytics/reporting
- All program metadata changes captured in audit trail
- User-program association model supports per-program role assignments

## Key SRS References

- Section 3.8.1 — Care Program Management (identification, metadata, changes, ORH, training programs, roster access)
- Section 5.2.2 — Program Search and Editing
- Section 4.2 — Patient-program association

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 02-001 | [Care Program Data Model](stories/02-001-care-program-data-model.md) | P0 | None |
| 02-002 | [Care Program CRUD API](stories/02-002-care-program-crud-api.md) | P0 | 02-001 |
| 02-003 | [Care Program Management UI](stories/02-003-care-program-management-ui.md) | P0 | 02-002, 01-004 |
| 02-004 | [Orphaned Record Holding](stories/02-004-orphaned-record-holding.md) | P0 | 02-001 |
| 02-005 | [Training Programs](stories/02-005-training-programs.md) | P1 | 02-001 |
| 02-006 | [Program Audit Logging](stories/02-006-program-audit-logging.md) | P0 | 02-002, 12-001 |
| 02-007 | [Program-User Association](stories/02-007-program-user-association.md) | P0 | 02-001, 01-004 |
