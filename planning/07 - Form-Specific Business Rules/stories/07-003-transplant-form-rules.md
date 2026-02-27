# Transplant Form Rules

**Story ID:** 07-003
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.3

## User Story
As a care program user, I want transplant forms to track organ-level transplant events with step progression so that each transplant journey is properly documented.

## Description
A patient can have zero, one, or multiple Transplant forms — one per organ-transplant event. Each form tracks transplant steps: referral, evaluation, waitlist, transplantation. Cannot create a new Transplant form for the same Organ if an existing form for that Organ does not include a 'Had Transplantation' step. Complete status is user-specified.

## Dependencies
- 06-001

## Acceptance Criteria
- [ ] Patient can have zero, one, or multiple Transplant forms
- [ ] Each form tracks one organ's transplant journey with steps
- [ ] Cannot create new form for same Organ if existing form lacks 'Had Transplantation' step
- [ ] Duplicate organ validation enforced at creation time
- [ ] Complete status is user-specified (not automatic)
- [ ] Transplant forms are shared (editable by any associated program)

## Technical Notes
- Transplant step progression is: Referral → Evaluation → Waitlist → Transplantation. A second lung transplant is a valid scenario.
