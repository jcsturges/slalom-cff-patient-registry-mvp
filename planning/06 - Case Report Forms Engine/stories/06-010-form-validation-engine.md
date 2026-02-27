# Form Validation Engine

**Story ID:** 06-010
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.5.1

## User Story
As a care program user, I want to receive clear, categorized error messages when I enter invalid data so that I understand what needs to be fixed and whether it blocks saving or completion.

## Description
4-tier validation system: (1) Warning — non-blocking, value outside expected range, (2) Completion-blocking — prevents Mark Complete but allows Save, (3) Save-blocking — prevents any save, (4) Dependency change warning — confirms before clearing dependent data. Validation messages identify: field(s) involved, rule violated, corrective action. UI clearly distinguishes severity levels with consistent indicators.

## Dependencies
- 06-007 (Field Type Support)

## Acceptance Criteria
- [ ] Warnings display for out-of-range values but don't prevent save or complete
- [ ] Completion-blocking errors prevent Mark Complete but allow Save as Incomplete
- [ ] Save-blocking errors prevent any save and present list of required corrections
- [ ] All validation messages identify field, rule violated, and corrective action
- [ ] UI uses consistent icons/labels to distinguish warning vs completion-blocking vs save-blocking
- [ ] Validation messages are navigable (inline field message and/or summary panel with links)
- [ ] Validation executes in real-time as user enters data

## Technical Notes
- Build a generic validation engine that accepts rule definitions (per form type) and returns categorized results.
