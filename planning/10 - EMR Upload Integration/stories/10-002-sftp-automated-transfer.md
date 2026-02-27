# SFTP Automated Transfer

**Story ID:** 10-002
**Epic:** 10 - EMR Upload Integration
**Priority:** P2
**SRS Reference:** Section 9

## User Story
As a system administrator, I want to configure automated SFTP transfers from care program EMR systems so that data flows automatically at defined intervals without manual uploads.

## Description
Data flows automatically at defined intervals without manual uploads

## Dependencies
- 10-001

## Acceptance Criteria
- [ ] SFTP endpoint configurable per care program
- [ ] Scheduled import at defined intervals
- [ ] Import triggers same processing pipeline as manual CSV upload
- [ ] Transfer logs capture: timestamp, program, file size, record count, success/failure
- [ ] Failed transfers generate alerts

## Technical Notes
- SFTP infrastructure is a significant operational concern — may require separate infrastructure.
