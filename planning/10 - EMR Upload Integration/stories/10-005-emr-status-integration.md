# EMR Status Integration

**Story ID:** 10-005
**Epic:** 10 - EMR Upload Integration
**Priority:** P1
**SRS Reference:** Section 6.2.1.2

## User Story
As a system, I want to correctly handle form status when EMR data updates a form so that users are prompted to review and re-validate EMR-driven changes.

## Description
Users are prompted to review and re-validate EMR-driven changes

## Dependencies
- 06-002, 06-012

## Acceptance Criteria
- [ ] EMR-driven updates trigger form status downgrade to Incomplete
- [ ] Banner: 'This form was updated from EMR and requires review. Please validate and Mark Complete.'
- [ ] User must re-validate and Mark Complete
- [ ] Demographics auto-complete applies even after EMR update
- [ ] EMR update source tracked to distinguish from user changes

## Technical Notes
- Links to the generic EMR status downgrade behavior in 06-012.
