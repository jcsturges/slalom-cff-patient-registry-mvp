# Add Existing Patient (Re-acquisition)

**Story ID:** 04-006
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P0
**SRS Reference:** Sections 4.3.1.2, 4.3.1.4

## User Story
As a care program editor, I want to add an existing patient (including from ORH) to my program's roster so that patients transferring between programs can continue care.

## Description
When duplicate detection finds matches, the user can select an existing patient from the match list instead of creating a new record. Selecting a match adds the patient to the user's currently selected Program Roster by creating a patient-program association (if one doesn't already exist). If the patient was in ORH, re-acquisition removes the orphan condition. After re-acquisition, the user is required to review the Demographics form to confirm data is correct and update newly added fields.

## Dependencies
- 04-005 (Duplicate Detection)

## Acceptance Criteria
- [ ] Selecting an existing match from the match list creates a patient-program association
- [ ] If association already exists, system notifies user (no duplicate association)
- [ ] Re-acquired ORH patients have orphan condition removed (if now has ≥1 clinical association)
- [ ] After re-acquisition, user is directed to review Demographics form
- [ ] Patient appears on the program's roster after re-acquisition
- [ ] Re-acquisition is logged in the audit trail

## Technical Notes
- Re-acquisition from ORH creates a new clinical association; ORH association removal logic in 02-004
- Demographics review is mandatory (redirect to Demographics form with review prompt)
