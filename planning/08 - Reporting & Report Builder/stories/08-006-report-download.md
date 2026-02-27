# Report Download

**Story ID:** 08-006
**Epic:** 08 - Reporting & Report Builder
**Priority:** P0
**SRS Reference:** Section 7.2.3

## User Story
As a care program user, I want to download report results as CSV or Excel files so that I can analyze data in external tools.

## Description
I can analyze data in external tools

## Dependencies
- 08-005

## Acceptance Criteria
- [ ] Download as CSV or Excel format
- [ ] Header row includes report title and date/timestamp
- [ ] File contains all results (not limited by pagination)
- [ ] All downloads logged: Report Name, Program Number, email, fields, patient count, timestamp
- [ ] Foundation Admin can access download logs

## Technical Notes
- Large report downloads should be handled asynchronously with notification when ready.
