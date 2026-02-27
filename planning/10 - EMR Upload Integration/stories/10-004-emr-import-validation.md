# EMR Import Validation

**Story ID:** 10-004
**Epic:** 10 - EMR Upload Integration
**Priority:** P1
**SRS Reference:** Section 9

## User Story
As a system, I want to validate EMR import files at both file and field level so that invalid data is rejected with clear error reporting rather than silently imported.

## Description
Invalid data is rejected with clear error reporting rather than silently imported

## Dependencies
- 10-003

## Acceptance Criteria
- [ ] File-level validation: reject entirely invalid files with clear error
- [ ] Field-level validation: reject individual invalid fields with error detail
- [ ] Hierarchical error checking: most critical errors checked first
- [ ] Form control logic for: blank fields, missing values, clearing values, updates
- [ ] Warning messages for informational issues (non-blocking)
- [ ] Error report available to uploading user

## Technical Notes
- Error reporting should be detailed enough for EMR teams to fix their export configuration.
