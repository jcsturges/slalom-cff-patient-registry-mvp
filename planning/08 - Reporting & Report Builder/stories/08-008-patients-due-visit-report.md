# Patients Due Visit Report

**Story ID:** 08-008
**Epic:** 08 - Reporting & Report Builder
**Priority:** P1
**SRS Reference:** Section 7.5.1

## User Story
As a care program user, I want to see which patients are due for a visit based on their last encounter date so that I can prioritize patient outreach and scheduling.

## Description
I can prioritize patient outreach and scheduling

## Dependencies
- 06-001

## Acceptance Criteria
- [ ] One row per patient in the program roster
- [ ] Sorted by most recent encounter date (user's program)
- [ ] Highlight patients not seen at user's program for >180 days
- [ ] Highlight patients not seen at any program for >2 years
- [ ] Columns include last seen dates for specialists (Dietitian, Pharmacist, PT, RT, SW, Mental Health)
- [ ] Report respects locked/unlocked year encounter status rules
- [ ] Diagnosis filter: CF only vs all diagnoses
- [ ] Time period filter: patients seen within specified period

## Technical Notes
- Complex calculated columns — consider pre-computing 'last seen' dates.
