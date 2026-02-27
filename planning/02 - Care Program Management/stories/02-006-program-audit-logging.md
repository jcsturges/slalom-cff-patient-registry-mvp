# Program Audit Logging

**Story ID:** 02-006
**Epic:** 02 - Care Program Management
**Priority:** P0
**SRS Reference:** Section 3.8.1.3

## User Story
As a CF Foundation Administrator, I want all program metadata changes to be captured in an audit log so that I can track who changed what and when.

## Description
All changes to care program metadata must be captured in an audit trail. Each audit entry records: CF care program ID, field changed (old → new), timestamp, and the CF Foundation Administrator user who made the change.

## Dependencies
- 02-002 (Care Program CRUD API)
- 12-001 (Application Audit Logging)

## Acceptance Criteria
- [ ] Every program metadata change generates an audit log entry
- [ ] Audit entry includes: Program ID, field name, old value, new value, timestamp, acting user ID
- [ ] Audit log is queryable by program ID, date range, and user
- [ ] Audit log entries are immutable (cannot be edited or deleted)
- [ ] Program creation is logged as a special "Created" event with all initial values
- [ ] Deactivation events are clearly identifiable in the log

## Technical Notes
- Leverage the cross-cutting audit logging infrastructure (Epic 12)
- Consider using EF Core change tracking interceptors to auto-generate audit entries
- Audit log table: `program_audit_log(id, program_id, field_name, old_value, new_value, action, user_id, timestamp)`
