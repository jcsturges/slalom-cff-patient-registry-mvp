# Hard-Delete Patient Record

**Story ID:** 05-005
**Epic:** 05 - Patient Dashboard & Record Management
**Priority:** P1
**SRS Reference:** Section 5.1.2

## User Story
As a CF Foundation Administrator, I want to permanently delete a patient record so that records created in error can be completely removed from the system.

## Description
Foundation Admins can hard-delete a patient from the action menu in the Patient Dashboard. A confirmation prompt requires the user to type the CFF ID to confirm deletion. Hard-delete completely erases all patient data from the Registry, including all forms, files, program associations (including ORH), and metadata. The deletion event is tracked in the audit log.

## Dependencies
- 05-003 (Patient Search — Admin)

## Acceptance Criteria
- [ ] Delete button visible only to Foundation Administrators in Patient Dashboard action menu
- [ ] Confirmation prompt requires entering the CFF ID to confirm
- [ ] Hard-delete removes ALL data: forms, files, program associations, ORH association, metadata
- [ ] Deleted patient does not appear in any search, roster, or report
- [ ] Deletion event logged in audit trail (CFF ID, timestamp, acting user)
- [ ] Delete operation is irreversible

## Technical Notes
- Use cascading delete across all related tables
- Consider soft-delete with a grace period before permanent removal (if CFF prefers)
- Audit log entry must NOT contain PHI — only CFF ID and operation metadata
