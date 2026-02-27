# Merge Duplicate Records — Foundation Admin

**Story ID:** 04-009
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P1
**SRS Reference:** Section 4.3.3.2

## User Story
As a CF Foundation Administrator, I want to merge any two patient records in the system so that duplicate records across programs can be consolidated.

## Description
Foundation Admins access "Merge Duplicate Records" from their menu. They can search across all patients in the Registry (including ORH, excluding consent-withdrawn). Same workflow as CP Admin merge (select primary/secondary, side-by-side review, confirm) but with system-wide scope and no multi-program restriction.

## Dependencies
- 04-008 (Merge Records — CP Admin)
- 05-003 (Patient Search — Admin)

## Acceptance Criteria
- [ ] Foundation Admins can access Merge Duplicate Records from their dedicated menu
- [ ] Search across all patients system-wide (including ORH)
- [ ] Excludes consent-withdrawn patients
- [ ] Exact match search by CFF ID, first name, or last name
- [ ] Can merge patients associated with multiple programs
- [ ] Same side-by-side review and confirmation workflow as CP Admin merge

## Technical Notes
- Reuses the same merge engine as CP Admin merge, with elevated search scope
- Foundation Admin merge must handle cross-program form retention rules
