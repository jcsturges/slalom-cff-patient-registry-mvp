# EMR Status Downgrade

**Story ID:** 06-012
**Epic:** 06 - Case Report Forms Engine
**Priority:** P1
**SRS Reference:** Section 6.2.1.2

## User Story
As a system, I want to automatically downgrade form status when EMR data updates a form so that users are prompted to review and re-validate changes.

## Description
When any field on a form is created/updated/deleted by an EMR-driven update, the system automatically sets completion status to Incomplete (regardless of whether completion criteria remain met). A banner displays: 'This form was updated from EMR and requires review. Please validate and Mark Complete.' The form must pass completion validation again — user must click 'Mark Complete' or 'Save and Mark Complete'. For Demographics: auto-set to Complete if all required fields are filled after EMR update.

## Dependencies
- 06-002 (Form Status Management), 10-003 (EMR Field Mapping)

## Acceptance Criteria
- [ ] EMR-driven updates to any form field trigger status downgrade to Incomplete
- [ ] Banner displayed: 'This form was updated from EMR and requires review...'
- [ ] User must re-validate and explicitly Mark Complete
- [ ] Demographics: auto-complete behavior applies even after EMR update
- [ ] EMR status downgrade applies to all Encounter forms and Labs & Tests forms
- [ ] Status downgrade happens regardless of whether completion criteria are still met

## Technical Notes
- The EMR update source must be tracked (e.g., `last_update_source` field) to distinguish user vs EMR changes.
