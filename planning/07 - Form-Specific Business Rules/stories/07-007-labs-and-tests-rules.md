# Labs & Tests Rules

**Story ID:** 07-007
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.5

## User Story
As a care program user, I want Labs & Tests forms to follow the same sub-form pattern as Encounters so that lab data entry is consistent and organized.

## Description
Labs & Tests forms follow the same sub-form (tab) architecture as Encounters. Sub-forms are optional blocks that users select at creation. Sub-form completion is automatic when at least one field in each sub-form is completed (including conditional fields enabled by dependencies). Overall completion is user-specified. One form per patient per day (Lab Date).

## Dependencies
- 07-005

## Acceptance Criteria
- [ ] Labs & Tests forms support selectable sub-forms like Encounters
- [ ] Sub-form completion automatic when ≥1 field in each sub-form completed
- [ ] Conditional fields that become enabled based on dependencies count toward completion
- [ ] Overall completion requires all sub-forms complete, then user-specified
- [ ] One Labs form per patient per day (Lab Date uniqueness)
- [ ] Lab Date is required to save

## Technical Notes
- Labs sub-form structure may differ from Encounter sub-forms — form schema definitions will specify which tabs are available.
