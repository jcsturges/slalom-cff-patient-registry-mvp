# Form Data Model

**Story ID:** 06-001
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.1

## User Story
As a system architect, I want to define the data model for all 10 case report form types so that forms can be created, stored, and retrieved with correct ownership and access rules.

## Description
Data model for 10 CRF types: Demographics, Diagnosis, Sweat Test & Fecal Elastase, Transplant, ALD Initiation (shared forms); Annual Review, Encounter, Labs and Tests, Care Episode, Phone Note (program-specific). Each form instance has: patient_id, program_id (for program-specific), form_type, completion_status, lock_status, created/updated metadata. Shared forms are editable by any associated program; program-specific forms are editable only by the reporting program.

## Dependencies
- 04-001 (Patient Data Model)

## Acceptance Criteria
- [ ] Form entity supports all 10 CRF types with correct shared/program-specific classification
- [ ] Each form instance tracks: patient_id, form_type, owning_program_id, completion_status, lock_status
- [ ] Shared forms accessible by all programs associated with the patient
- [ ] Program-specific forms editable only by the reporting program, read-only by others
- [ ] Annual Review forms are not accessible by other programs at all
- [ ] Form instances track created_at, created_by, updated_at, updated_by

## Technical Notes
- Consider a single `Forms` table with a `form_type` discriminator or separate tables per form type. JSON schema definitions can drive dynamic rendering.
