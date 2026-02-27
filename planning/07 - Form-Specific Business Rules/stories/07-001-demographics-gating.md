# Demographics Gating

**Story ID:** 07-001
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.1

## User Story
As a system, I want to enforce that Demographics must be Complete before any other forms can be created so that all patients have baseline identity data.

## Description
Demographics is the gating prerequisite for ALL other forms. Auto-complete status assigned when required fields are filled. System re-evaluates completeness on every save and EMR update. If Demographics is not complete, clicking Patient Dashboard link redirects to Demographics form with a message about required fields. When adding an existing patient, user must review Demographics.

## Dependencies
- 06-001, 06-002

## Acceptance Criteria
- [ ] No other CRFs can be created until Demographics status = Complete
- [ ] Demographics auto-completes when all required fields have valid entries
- [ ] Completeness re-evaluated on every save (user or EMR)
- [ ] If Demographics incomplete, Patient Dashboard link redirects to Demographics form
- [ ] Redirect shows clear message about which required fields need completion
- [ ] When adding existing patient to program, user is redirected to review Demographics

## Technical Notes
- This is the most critical form gating rule — must be enforced at both API and UI level.
