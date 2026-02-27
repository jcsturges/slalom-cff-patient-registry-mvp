# Training Programs

**Story ID:** 02-005
**Epic:** 02 - Care Program Management
**Priority:** P1
**SRS Reference:** Section 3.8.1.5

## User Story
As a CF Foundation Administrator, I want to create training programs that mirror real program functionality but are excluded from analytics and reporting so that users can practice without contaminating production data.

## Description
Foundation Admins can create multiple programs flagged as Training Programs. These programs provide full workflow equivalence for assigned users (data entry, forms, reporting within the program) but are deterministically excluded from all analytics, reporting-year calculations, downstream feeds, patient merge, and patient search. Exclusion is enforced via the `IsTrainingProgram = true` flag.

## Dependencies
- 02-001 (Care Program Data Model)

## Acceptance Criteria
- [ ] Foundation Admins can create programs with `IsTrainingProgram = true`
- [ ] Training Programs provide full workflow functionality for assigned users
- [ ] Training Program IDs do not share ID space with clinical programs and do not reuse retired IDs
- [ ] All data from Training Programs is excluded from: analytical datasets, reporting-year calculations, downstream feeds, patient merge, patient search
- [ ] Exclusion is deterministic via the `IsTrainingProgram` flag (cannot be accidentally included)
- [ ] Training Programs can be activated/deactivated by Foundation Admins
- [ ] Users assigned to Training Programs can enter fake patient data for practice
- [ ] Training environment has clear visual indicator distinguishing it from production

## Technical Notes
- All report queries, data feeds, and merge operations must include a `WHERE IsTrainingProgram = false` filter
- Consider a global query filter in EF Core to exclude training programs by default
- Training Programs may need a distinct visual treatment in the UI to prevent confusion
