# Annual Review Rules

**Story ID:** 07-004
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.4

## User Story
As a care program user, I want Annual Review forms to default to the correct year and enforce one-per-patient-per-program-per-year so that annual data collection is properly organized.

## Description
Annual Review has a year dropdown. Default year: most recent unlocked reporting year without existing form for that patient/program. One per patient per program per year. Cannot set to future years or locked years. Carry-forward: Demographics, Diabetes Status, socio-economic status sections from preceding year's form (same program), independent of completion status.

## Dependencies
- 06-001, 06-003

## Acceptance Criteria
- [ ] Year dropdown defaults to most recent unlocked year without existing form
- [ ] One Annual Review per patient per program per year
- [ ] Cannot create for future years or locked years
- [ ] If both current and prior year are unlocked, default to current year first
- [ ] Carry-forward from preceding year: Demographics, Diabetes Status, socio-economic fields
- [ ] Carry-forward pulls from same program, independent of completion status
- [ ] Carry-forward pre-fills fields; user can edit as if manually entered
- [ ] Complete status is user-specified

## Technical Notes
- Year defaulting logic example: On Feb 1 2027, both 2026 and 2027 are open → default = 2027 if no 2027 form exists; otherwise default = 2026 if no 2026 form exists.
