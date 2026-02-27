# Form Status Management

**Story ID:** 06-002
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.2.1

## User Story
As a system, I want to track and enforce form completion status with auto-complete rules so that data quality can be monitored and reported.

## Description
Implement the Complete/Incomplete status system. Some forms auto-complete when required fields are filled (Demographics, Diagnosis, Sweat Test, Encounter sub-forms, Labs sub-forms). Others require user to explicitly 'Mark Complete' (ALD Initiation, Transplant, Annual Review, Care Episode, Encounter overall, Labs overall). Status re-evaluated on every save. If user edits a Complete form and required fields become missing, status reverts to Incomplete.

## Dependencies
- 06-001 (Form Data Model)

## Acceptance Criteria
- [ ] Auto-complete works for: Demographics, Diagnosis, Sweat Test, Encounter sub-forms, Labs sub-forms
- [ ] User-specified complete for: ALD Initiation, Transplant, Annual Review, Care Episode, Encounter overall, Labs overall
- [ ] Editing a Complete form re-evaluates completion criteria
- [ ] If criteria no longer met after edit, status reverts to Incomplete
- [ ] If criteria still met after edit, status remains Complete (no automatic downgrade)
- [ ] Form status updates can be made by any CP Admin or Editor associated with the program

## Technical Notes
- Completion criteria are form-specific — defined in Epic 07 stories. This story implements the generic engine.
