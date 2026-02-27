# Diagnosis Gating

**Story ID:** 07-002
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.2

## User Story
As a system, I want to enforce that Diagnosis must be Complete before most clinical forms can be entered so that patients have a confirmed diagnosis before clinical data collection.

## Description
Diagnosis is the secondary gate after Demographics. No additional CRFs (except Sweat Test/Fecal Elastase and file uploads) allowed without Complete Diagnosis. If Demographics Complete but no Diagnosis form exists, Dashboard redirects to Diagnosis. Auto-complete requires: at least one Diagnosis History repeat block entry + all Neonatal History fields selected. When adding existing patient, user must review Diagnosis.

## Dependencies
- 07-001, 06-008

## Acceptance Criteria
- [ ] No CRFs (except Sweat Test and file uploads) allowed without Complete Diagnosis
- [ ] If Demographics Complete but Diagnosis missing, Dashboard redirects to Diagnosis
- [ ] Auto-complete: ≥1 Diagnosis History repeat block + all Neonatal History fields
- [ ] Sweat Test and Fecal Elastase accessible regardless of Diagnosis status
- [ ] File uploads permitted without Complete Diagnosis
- [ ] When adding existing patient, must review Diagnosis and confirm/update

## Technical Notes
- Diagnosis gating is secondary to Demographics — both must be Complete before most clinical forms.
