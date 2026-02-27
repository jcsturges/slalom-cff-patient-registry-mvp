# Field Dependencies

**Story ID:** 06-011
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.5.1.5

## User Story
As a care program user, I want to be warned before changing a field that would clear dependent data so that I don't accidentally lose entered information.

## Description
When a parent field changes and dependent fields become disabled, warn the user that dependent data will be cleared. Require explicit confirmation before discarding. Conditional required fields: when a parent value triggers a dependency, the dependent field becomes required.

## Dependencies
- 06-010 (Form Validation Engine)

## Acceptance Criteria
- [ ] Changing a parent field that disables dependent fields triggers a warning
- [ ] Warning clearly identifies which dependent data will be cleared
- [ ] User must explicitly confirm before dependent data is discarded
- [ ] If user cancels, the parent field change is reverted
- [ ] Conditional required fields become required when parent trigger condition is met
- [ ] Conditional required fields become optional/hidden when parent trigger is not met

## Technical Notes
- Dependency rules should be defined in the form schema alongside field definitions.
