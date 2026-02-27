# Report Builder — Patient Selection

**Story ID:** 08-002
**Epic:** 08 - Reporting & Report Builder
**Priority:** P0
**SRS Reference:** Section 7.3.2

## User Story
As a care program user, I want to build complex patient selection queries using include/exclude blocks with visual condition builders so that I can define exactly which patients to include in my report.

## Description
I can define exactly which patients to include in my report

## Dependencies
- 08-001, 06-001

## Acceptance Criteria
- [ ] Include/exclude blocks with AND/AND NOT logic between blocks
- [ ] Conditions within a block connected with AND
- [ ] Any data field from any form selectable as filter criteria
- [ ] Fields organized by form/sub-form in searchable dropdown
- [ ] Appropriate operators per field type (numeric, enum, boolean, string, date)
- [ ] Date range options for encounter-based forms (any/all/some, ever/last 2 years/selected range)
- [ ] Complete forms only (default) or All forms option

## Technical Notes
- Most complex UI component in the system. Consider a visual query builder library.
