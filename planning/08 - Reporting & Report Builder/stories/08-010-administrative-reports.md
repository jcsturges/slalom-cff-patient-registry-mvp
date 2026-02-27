# Administrative Reports

**Story ID:** 08-010
**Epic:** 08 - Reporting & Report Builder
**Priority:** P1
**SRS Reference:** Sections 7.6.1-7.6.5

## User Story
As a CF Foundation Administrator, I want to run administrative reports for program oversight so that I can monitor program activity, data uploads, merges, transfers, and file uploads across the network.

## Description
I can monitor program activity, data uploads, merges, transfers, and file uploads across the network

## Dependencies
- 01-004

## Acceptance Criteria
- [ ] CF Care Program List: all programs with stats (active/deactivated users, patients, data entry dates)
- [ ] Duplicate Record Merge Report: primary/secondary IDs, user, merge date
- [ ] EMR Data Upload Report: per-program SFTP/manual upload counts for demographics/encounters/labs
- [ ] Patient Transfer Report: CFF ID, program, user, transaction type (add/remove/consent), date
- [ ] File Upload Reports: all files by program with metadata, filterable by date range/program/file type
- [ ] All reports support pagination, sort, search, and CSV/Excel download
- [ ] Reports accessible only to Foundation Administrators

## Technical Notes
- Five distinct administrative reports — consider a shared admin report framework.
