# Phone Note Rules

**Story ID:** 07-010
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.5

## User Story
As a care program user, I want to enter Phone Notes with a unique date per program so that phone-based clinical interactions are tracked.

## Description
Phone Note is a simple program-specific form with a Phone Note Date. One per patient per day per program (date uniqueness enforced). Program-specific: editable only by reporting program, read-only by other associated programs.

## Dependencies
- 06-001

## Acceptance Criteria
- [ ] Phone Note Date is required to save
- [ ] One Phone Note per patient per day per program
- [ ] Duplicate date validation on save
- [ ] Program-specific: editable only by reporting program
- [ ] Read-only access for other associated programs
- [ ] Completion status follows standard rules

## Technical Notes
- Simplest of the program-specific forms. Good candidate for early implementation to validate the form engine.
