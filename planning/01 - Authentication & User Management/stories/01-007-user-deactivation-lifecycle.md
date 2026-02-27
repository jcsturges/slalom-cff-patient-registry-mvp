# User Deactivation Lifecycle

**Story ID:** 01-007
**Epic:** 01 - Authentication & User Management
**Priority:** P1
**SRS Reference:** Section 2.3.5

## User Story
As a system administrator, I want deactivated users to be properly handled across programs — preserved in historical records but prevented from new actions — so that audit trails are maintained while access is revoked.

## Description
Implement program-specific user deactivation states. A user can be active in one program and deactivated in another without affecting access elsewhere. Deactivated users remain referenced in historical records (audit logs, "last modified by" fields, etc.) to preserve data integrity. In the UI, deactivated users are labeled "Deactivated" and cannot be selected for new actions or assignments. When a user is deactivated from all programs, the system connects to the my.cff API to remove them from the access group.

## Dependencies
- 01-005 (CP Admin User Management)
- 01-006 (Foundation Admin User Management)

## Acceptance Criteria
- [ ] Deactivation is per-program — active in one program and deactivated in another is supported
- [ ] Deactivated users are preserved in historical records (audit logs, modification history)
- [ ] Deactivation does not delete or alter prior actions or data
- [ ] Deactivated users are labeled "Deactivated" (or equivalent) in the UI
- [ ] Deactivated users are not selectable for new actions or assignments
- [ ] When deactivated from all CF care programs, system calls my.cff API to remove from access group
- [ ] Re-activation is possible (if supported by business rules)
- [ ] All deactivation/reactivation events are logged

## Technical Notes
- User-program association record needs an `active` status field
- "Last modified by" references should use user ID (not name) to handle name changes
- Consider soft-delete pattern: `deactivated_at`, `deactivated_by` fields on the association
