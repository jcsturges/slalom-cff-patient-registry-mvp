# Patient Data Model

**Story ID:** 04-001
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P0
**SRS Reference:** Section 4.1

## User Story
As a system architect, I want a comprehensive patient data model with CFF ID, metadata fields, and calculated fields so that patient records support all Registry workflows.

## Description
The patient entity includes: CFF ID (unique numeric, auto-assigned for new patients, migrated for existing), first/middle/last name, last name at birth, date of birth, biological sex at birth, last 4 SSN (optional), and multiple calculated fields derived from clinical data and platform metadata.

**Calculated fields from CRFs:** Most recent CF Diagnosis, Vital status (Alive/Deceased), ALD status, Has Lung Transplant, Last Seen in Program, Current age.

**Calculated from platform metadata:** Last Modified Date/By, Last Modified By Program, Status of Annual Review, Number of Unlocked/Incomplete forms.

**From merge operations:** Registry IDs of secondary records (aliases), Past names.

## Dependencies
- 02-001 (Care Program Data Model)

## Acceptance Criteria
- [ ] CFF ID is unique, numeric, auto-assigned for new patients
- [ ] Patient entity stores: first, middle, last name, last name at birth, DOB, biological sex, last 4 SSN
- [ ] Calculated fields correctly derive from CRF data (diagnosis, vital status, ALD, transplant)
- [ ] Platform metadata fields track: last modified date/by/program, annual review status, incomplete form count
- [ ] Alias storage for merged record IDs and past names
- [ ] CFF ID generation avoids collisions and supports migration of existing IDs

## Technical Notes
- Expand current Patient model significantly; current MVP model is minimal
- Calculated fields should be materialized (stored, updated on write) for query performance
- Consider a `PatientAlias` table for merged record IDs and past names
