# Epic 04: Program Roster & Patient Identity

## Overview

This epic covers the core patient data model, the Program Roster (the primary list view for care program users), the Add Patient workflow with duplicate detection, the Remove Patient workflow with consent handling, and the patient record merge functionality.

The Program Roster is the central working interface for care program users — it displays all patients associated with their selected care program. From here, users can search for patients, add new patients (with sophisticated duplicate detection), remove patients (with consent-aware workflows), and initiate record merges. The patient identity system assigns unique CFF IDs and tracks calculated fields derived from clinical data entry.

Patient-program associations are the backbone of the data model, controlling roster membership, data access permissions, and reporting scope. The Orphaned Record Holding (ORH) system ensures no patient record is ever lost.

## Outcomes

- Patient data model supports CFF ID, all metadata fields, and calculated fields
- Patient-program association model enables multi-program membership with ORH safety net
- Program Roster displays all required columns with search, sort, and pagination
- Add Patient workflow captures identity fields with consent attestation
- Duplicate detection engine implements 8 fuzzy matching rules with configurable thresholds
- Existing patients (including ORH) can be re-acquired to a program roster
- Remove Patient workflow supports 3 removal reasons with ORH auto-association
- Consent withdrawal triggers cross-program removal with notifications
- Care Program Admin merge: within-program records with side-by-side review
- Foundation Admin merge: system-wide with identical workflow
- Merge consolidation: metadata aliasing, form retention rules, file reassignment

## Key SRS References

- Section 4.1 — Patient data and metadata fields
- Section 4.2 — Patient-program association
- Section 4.3 — Program Roster
- Section 4.3.1 — Add patient to program workflow (Identity Form, duplicate detection, matching logic)
- Section 4.3.2 — Remove Patient from Program (consent handling)
- Section 4.3.3 — Merge duplicate patient records

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 04-001 | [Patient Data Model](stories/04-001-patient-data-model.md) | P0 | 02-001 |
| 04-002 | [Patient-Program Association](stories/04-002-patient-program-association.md) | P0 | 04-001, 02-004 |
| 04-003 | [Program Roster View](stories/04-003-program-roster-view.md) | P0 | 04-002, 03-010 |
| 04-004 | [Patient Identity Form](stories/04-004-patient-identity-form.md) | P0 | 04-001, 04-002 |
| 04-005 | [Duplicate Detection](stories/04-005-duplicate-detection.md) | P0 | 04-004 |
| 04-006 | [Add Existing Patient](stories/04-006-add-existing-patient.md) | P0 | 04-005 |
| 04-007 | [Remove Patient from Program](stories/04-007-remove-patient-from-program.md) | P0 | 04-002 |
| 04-008 | [Merge Records — CP Admin](stories/04-008-merge-duplicate-records-cp.md) | P1 | 04-003 |
| 04-009 | [Merge Records — Foundation Admin](stories/04-009-merge-duplicate-records-admin.md) | P1 | 04-008, 05-003 |
| 04-010 | [Merge Data Consolidation](stories/04-010-merge-data-consolidation.md) | P1 | 04-008 |
