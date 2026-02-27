# Care Program Data Model

**Story ID:** 02-001
**Epic:** 02 - Care Program Management
**Priority:** P0
**SRS Reference:** Section 3.8.1.1, 3.8.1.2

## User Story
As a system architect, I want to define the data model for care programs with all required metadata fields so that programs can be managed, searched, and associated with users and patients.

## Description
Design and implement the care program entity with all required fields:
- **Program ID** (integer, required, unique, immutable) — manually assigned, never reused
- **Program Name** (string, required)
- **Program Type** (enum: Pediatric, Adult, Affiliate, Orphaned-Record Holding, Training)
- **City** (string, optional)
- **State** (enum, US states, optional)
- **Display Title** (calculated, read-only): `<Program ID> - <Program Name> (<Program Type>)`
- **Active Status** (enum: Active, Inactive)
- **IsOrphanHoldingProgram** (boolean flag)
- **IsTrainingProgram** (boolean flag)

The system must enforce: no duplicate Program IDs, no changes to Program ID once assigned, no re-use of retired Program IDs, no deletion of programs.

## Dependencies
- None (foundational data model)

## Acceptance Criteria
- [ ] Care program entity includes all specified metadata fields
- [ ] Program ID is unique, immutable, and cannot be re-used
- [ ] Display Title is auto-calculated as `<ID> - <Name> (<Type>)`
- [ ] Program Type enum supports: Pediatric, Adult, Affiliate, Orphaned-Record Holding, Training
- [ ] Active Status supports: Active, Inactive
- [ ] Boolean flags: IsOrphanHoldingProgram, IsTrainingProgram
- [ ] Database constraints enforce uniqueness and immutability of Program ID
- [ ] Programs cannot be deleted (only deactivated)

## Technical Notes
- Current MVP `CareProgram` model needs significant expansion (missing: Program Type, State, flags, display title)
- Program ID should be a natural key assigned by Foundation Admins, not an auto-increment
- Consider a separate `ProgramFlags` table or a JSON column for extensibility
