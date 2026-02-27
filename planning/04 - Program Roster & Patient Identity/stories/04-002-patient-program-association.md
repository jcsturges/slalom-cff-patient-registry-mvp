# Patient-Program Association

**Story ID:** 04-002
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P0
**SRS Reference:** Sections 4.2, 4.2.1

## User Story
As a system architect, I want to model patient-program associations so that patients can belong to multiple programs and orphaned records are automatically safeguarded.

## Description
Patient-program association (PatientID + ProgramID) is the authoritative record for roster membership and program-level data context. A patient may have zero, one, or multiple associations. When a patient has no active clinical program associations, the ORH behavior (02-004) applies.

Each association has a status (Active, Removed) and tracks removal reason and timestamp. The association determines which care program users can access the patient's data.

## Dependencies
- 04-001 (Patient Data Model)
- 02-004 (Orphaned Record Holding)

## Acceptance Criteria
- [ ] Association model: patient_id, program_id, status (Active/Removed), removal_reason, timestamps
- [ ] A patient can have zero, one, or multiple program associations
- [ ] Active associations determine roster membership
- [ ] When all clinical associations are removed, ORH auto-association is triggered
- [ ] Removal reasons supported: Patient no longer seen, Patient withdrew consent, Consent issue
- [ ] Association changes are logged in the audit trail

## Technical Notes
- Replace current simple `CareProgramId` FK on Patient with a proper junction table
- Association status transitions: Active → Removed (with reason)
- Consider soft-delete pattern to preserve history
