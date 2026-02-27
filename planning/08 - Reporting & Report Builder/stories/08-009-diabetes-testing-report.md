# Diabetes Testing Report

**Story ID:** 08-009
**Epic:** 08 - Reporting & Report Builder
**Priority:** P1
**SRS Reference:** Section 7.5.2

## User Story
As a care program user, I want to identify patients who need diabetes testing so that I can ensure eligible patients receive appropriate glucose monitoring.

## Description
I can ensure eligible patients receive appropriate glucose monitoring

## Dependencies
- 06-001

## Acceptance Criteria
- [ ] Include patients ≥10 years with no prior diabetes diagnosis (CFRD, Type 1, Type 2)
- [ ] Include patients with no OGTT results in 12 months preceding report execution
- [ ] Columns: CFF ID, name, DOB, most recent random/fasting glucose date and value, most recent OGTT date and values
- [ ] Sorted alphabetically by last name
- [ ] Report evaluates encounters per locked/unlocked year status rules

## Technical Notes
- Report requires cross-referencing Annual Review diagnosis data with Encounter lab data.
