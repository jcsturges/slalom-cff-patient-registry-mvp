# Report Builder — Management

**Story ID:** 08-004
**Epic:** 08 - Reporting & Report Builder
**Priority:** P0
**SRS Reference:** Section 7.3.4

## User Story
As a care program user, I want to save, share, and manage my report queries so that I can reuse them without re-entering criteria so that I build a report library over time.

## Description
I build a report library over time

## Dependencies
- 08-003

## Acceptance Criteria
- [ ] My Reports: full CRUD on own queries
- [ ] My Program's Reports: view, execute, Save As queries from same program
- [ ] Global Reports: execute, view CFF-created queries
- [ ] Foundation Admin: full control + scope filtering (owner, program, user)
- [ ] Saved query versioning (new version assigned to modifier, original preserved)
- [ ] Invalid field detection with notification and edit option
- [ ] Sort/filter report list by title, status, date created, date last executed

## Technical Notes
- Report definitions stored as JSON with schema versioning for forward compatibility.
