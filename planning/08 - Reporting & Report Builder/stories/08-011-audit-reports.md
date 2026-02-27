# Audit Reports

**Story ID:** 08-011
**Epic:** 08 - Reporting & Report Builder
**Priority:** P1
**SRS Reference:** Section 7.7

## User Story
As a CF Foundation Administrator, I want to run audit reports for user management and download activity so that I can monitor user access changes and data download activity across the Registry.

## Description
I can monitor user access changes and data download activity across the Registry

## Dependencies
- 12-001

## Acceptance Criteria
- [ ] User Management Report: user events (created, deactivated, role changed) within date range
- [ ] User Management Report columns: Program, name, email, event type, status, role, assigned by, date, other programs
- [ ] Download Details Report: all report/file downloads within date range
- [ ] Download Details Report columns: Report Name, type, CFF ID, program, email, role, fields, patient count, date
- [ ] Both reports support date range filtering
- [ ] Both reports close to real-time (minutes)
- [ ] Both reports accessible only to Foundation Administrators

## Technical Notes
- Audit reports depend on the logging infrastructure in Epic 12.
