# Report Usage Tracking

**Story ID:** 12-003
**Epic:** 12 - Audit Logging & Analytics
**Priority:** P1
**SRS Reference:** Section 3.8.8.2

## User Story
As a CF Foundation Administrator, I want to see report execution statistics so that I can monitor which reports are being used and by whom.

## Description
I can monitor which reports are being used and by whom

## Dependencies
- 12-001, 08-001

## Acceptance Criteria
- [ ] Every report execution logged
- [ ] Captures: report ID/name/version, user ID, timestamp
- [ ] Captures: parameters/filters used, output mode (on-screen/export/download)
- [ ] Captures: file format and size if exported
- [ ] Captures: success/failure status and runtime duration

## Technical Notes
- Report execution logs feed into the Download Details Report (08-011).
