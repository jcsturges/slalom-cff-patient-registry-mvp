# Form Deletion

**Story ID:** 06-004
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.2.2

## User Story
As a care program administrator, I want to delete unlocked forms that are no longer needed so that incorrect or unnecessary data is removed.

## Description
CP Admins/Editors and Foundation Admins can delete unlocked forms (confirmation required). Only Foundation Admins can delete shared forms (Demographics, Diagnosis, Sweat Test, ALD, Transplant). Deletion permanently removes content. Non-identifiable audit record retained. Deleted forms: not viewable, not modifiable, not in reports, not exportable.

## Dependencies
- 06-001 (Form Data Model), 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] Confirmation prompt before deletion
- [ ] Cannot undelete — deletion is permanent
- [ ] Only Foundation Admins can delete shared forms
- [ ] CP Admins/Editors can delete program-specific unlocked forms
- [ ] Locked forms cannot be deleted
- [ ] Deleted form content permanently removed from system
- [ ] Non-identifiable audit record retained (form type, deletion timestamp, user, no PHI)
- [ ] Deleted forms excluded from reports and exports
- [ ] Auto-delete triggered on merge per merge rules

## Technical Notes
- Consider soft-delete with a purge job for the non-identifiable audit record retention.
