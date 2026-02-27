# Field Type Support

**Story ID:** 06-007
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.5

## User Story
As a care program user, I want to enter data using appropriate field types so that clinical data is captured accurately.

## Description
Support all required field types: single-line text, multi-line text, numeric fields, date and date-time fields, radio button groups (single-select), multi-select and single-select checkboxes, drop-down lists. Each field type must support validation rules, required/optional designation, and tooltips.

## Dependencies
- 06-006 (Form Page Layout)

## Acceptance Criteria
- [ ] Single-line text fields with max length validation
- [ ] Multi-line text fields with max length validation
- [ ] Numeric fields with range validation
- [ ] Date and date-time pickers
- [ ] Radio button groups for single-select options
- [ ] Multi-select checkboxes and single-select checkboxes
- [ ] Drop-down lists with predefined options
- [ ] All field types support required/optional designation
- [ ] Field tooltips: visual icon indicates presence, hover/click displays help text
- [ ] Mutation field search: search-as-you-type dropdown with 2000+ options, mid-string matching

## Technical Notes
- Build a form field component library that renders based on JSON schema definitions. This supports the dynamic form engine pattern.
