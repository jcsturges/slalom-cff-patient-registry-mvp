# Calculated Fields

**Story ID:** 06-009
**Epic:** 06 - Case Report Forms Engine
**Priority:** P1
**SRS Reference:** Section 6.8.2

## User Story
As a care program user, I want to see automatically calculated clinical values so that I don't have to compute them manually.

## Description
Calculated read-only fields update automatically based on entered data. Patient-level: Current age (from DOB), Vital status (from date of death). Clinical: Percent predicted FEV1/FVC/FEV1FVC, Height/Weight/BMI percentiles (ages 0-18), Weight-for-length percentiles (0-2 years). Update within ≤2 seconds when inputs on same form change, ≤5 seconds when inputs on different form change. If required inputs missing/invalid, display blank or 'Not available'.

## Dependencies
- 06-007 (Field Type Support)

## Acceptance Criteria
- [ ] Current age calculated from DOB and current date
- [ ] Vital status derived from date of death field
- [ ] ppFEV1, FVC, FEV1/FVC calculated per reference equations
- [ ] Height/weight/BMI percentiles calculated for ages 0-18
- [ ] Weight-for-length percentiles calculated for ages 0-2
- [ ] Calculated within ≤2 seconds when same-form inputs change
- [ ] Updated within ≤5 seconds when different-form inputs change
- [ ] Missing/invalid inputs → blank or 'Not available'
- [ ] Calculated values updated in underlying data tables within ≤5 seconds

## Technical Notes
- Reference equations for pulmonary function and growth percentiles will need to be sourced from CFF's supplemental documents. Consider implementing as a server-side calculation service.
