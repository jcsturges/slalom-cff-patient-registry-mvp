# Database Lock Execution

**Story ID:** 06-005
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.2.3

## User Story
As a CF Foundation Administrator, I want to trigger the annual database lock that prevents editing of prior-year data so that reporting-year data is finalized.

## Description
The lock process locks forms based on date fields within the reporting year. Two execution modes: Fast Synchronous (<2 min) or Overnight Batch (2-6 AM ET). Active sessions preserved: skip in-progress forms, auto-lock on save. Post-lock: date validation restricts to current year, all Annual Reviews for locked year are Locked. Encounter/Labs/Episode/Phone locked by date within reporting year. Care Episodes with no end date not impacted.

## Dependencies
- 06-003 (Form Locking Status), 11-001 (Database Lock UI)

## Acceptance Criteria
- [ ] Foundation Admin triggers lock for preceding calendar year with exact date
- [ ] Fast synchronous mode completes in <2 minutes
- [ ] Overnight batch mode executes at 2:00 AM ET on scheduled date
- [ ] Active edit sessions are not interrupted — in-progress forms skipped
- [ ] When user saves a skipped form, it auto-locks with confirmation message
- [ ] Annual Review forms for locked year are all set to Locked
- [ ] Encounter/Labs/Episode/Phone forms locked based on date within reporting year
- [ ] Care Episode forms with no end date are not impacted
- [ ] Post-lock: date validation restricts new form dates to current year
- [ ] Users can create new forms during lock process (with date validation)

## Technical Notes
- This is a complex batch operation — consider a background job with progress tracking. The lock should be idempotent (safe to re-run).
