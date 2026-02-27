# Encounter Carry-Forward

**Story ID:** 07-006
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P1
**SRS Reference:** Section 6.6.5.1

## User Story
As a care program user, I want to carry forward medications and complications from the most recent prior Encounter so that I don't have to re-enter chronic conditions.

## Description
On initial access to Medications or Complications sub-form (tab) for a given Encounter, display a carry-forward prompt listing items eligible from the most recent prior Encounter (same program). Two actions: 'Carry Forward All' (pre-fills all items) or 'Cancel' (no carry-forward). Pre-filled data is editable. Prompt is one-time — dismissed after selection, never shown again for that sub-form instance.

## Dependencies
- 07-005

## Acceptance Criteria
- [ ] Carry-forward prompt shown on first access to Medications or Complications tab
- [ ] Prompt lists items from most recent prior Encounter (same program)
- [ ] Two actions: 'Carry Forward All' or 'Cancel'
- [ ] Carry Forward All pre-fills all relevant fields
- [ ] Pre-filled data is editable as if manually entered
- [ ] Prompt dismissed after selection (Carry Forward All or Cancel)
- [ ] Prompt never shown again for that sub-form instance (including on form reload)
- [ ] Source is most recent prior Encounter by date, regardless of completion status

## Technical Notes
- Store a flag per sub-form instance to track whether carry-forward prompt has been shown/dismissed.
