# Form Locking Status

**Story ID:** 06-003
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.2.3

## User Story
As a system, I want to enforce the Locked/Unlocked status on forms tied to the annual database lock so that prior-year data is protected from editing.

## Description
The Locked status is set by the system after the annual database lock (11-001). Locked form permissions: CP users cannot edit data or change completion status. Foundation Admins retain edit privileges on locked forms. Locked forms cannot be deleted.

## Dependencies
- 06-002 (Form Status Management), 11-001 (Database Lock UI)

## Acceptance Criteria
- [ ] Locked forms are read-only for CP users
- [ ] Foundation Admins can edit locked forms
- [ ] Locked forms cannot be deleted by any user
- [ ] Lock status is set automatically by the database lock process, not manually by users
- [ ] Complete+Locked and Incomplete+Locked states are both valid

## Technical Notes
- Lock status is orthogonal to completion status — both dimensions are independent.
