# Modify Program Association — Admin

**Story ID:** 05-004
**Epic:** 05 - Patient Dashboard & Record Management
**Priority:** P1
**SRS Reference:** Section 5.1.1

## User Story
As a CF Foundation Administrator, I want to modify patient-program associations in bulk so that I can manage patient transfers and organizational changes.

## Description
From Patient Search results, Foundation Admins can select one or more patients and update their program associations. Options include: remove from all programs (consent issue) → ORH, remove from all (inactivity) → ORH, remove from all (consent withdrawal) → ORH, add to another program (keep existing), transfer from one program to another, remove from specific program and assign to ORH. A free text field allows documentation of the reason. Confirmation screen shows summary (e.g., "3 patients shall be assigned to Program 7; are you sure?"). All changes tracked in audit log.

## Dependencies
- 05-003 (Patient Search — Admin)
- 04-002 (Patient-Program Association)

## Acceptance Criteria
- [ ] Foundation Admins can select one or more patients from search results
- [ ] All specified association modification options are available
- [ ] Free text field for reason documentation
- [ ] Confirmation screen with summary of changes
- [ ] Changes applied only after explicit confirmation
- [ ] All changes tracked in audit log with date, user ID, and reason text
- [ ] Consent withdrawal triggers cross-program removal + notification

## Technical Notes
- Batch operations should be transactional
- Consider a job queue for large batch operations to avoid request timeouts
