# Repeat Blocks

**Story ID:** 06-008
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.5

## User Story
As a care program user, I want to enter multiple instances of similar data within a form using repeat blocks so that I can capture all relevant clinical history.

## Description
Repeat blocks are groups of fields that can be repeated multiple times within a form. Each block is presented as a collapsible element. First block expanded on new form creation. Users can add additional blocks after filling existing ones. On save, all data in collapsed and uncollapsed blocks is saved. On reopen, previously filled blocks appear collapsed and summarized in a table. Users with edit privileges can open, edit, and delete previously filled blocks (unless parent form is locked).

## Dependencies
- 06-007 (Field Type Support)

## Acceptance Criteria
- [ ] Repeat blocks render as collapsible accordion elements
- [ ] First block expanded by default on new form
- [ ] Users can add additional blocks after filling existing ones
- [ ] All blocks (collapsed and expanded) saved on form save
- [ ] On reopen: filled blocks collapsed with table summary
- [ ] Users can open, edit, delete existing blocks (if not locked)
- [ ] Deleting a repeat block prompts for confirmation

## Technical Notes
- Repeat blocks are used heavily in Diagnosis (Diagnosis History), Encounter (medications), and other forms.
