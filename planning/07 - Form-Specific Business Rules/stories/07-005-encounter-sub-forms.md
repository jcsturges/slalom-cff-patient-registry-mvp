# Encounter Sub-Forms

**Story ID:** 07-005
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.5

## User Story
As a care program user, I want to select which sub-forms to include in an Encounter so that I only fill out the relevant clinical sections.

## Description
Encounter and Labs & Tests forms include sub-forms (tabs) that users select at creation. 'General Encounter Start' is always required as the landing page. Sub-forms: Medications, PFTs, GI/Nutrition, Complications, Lab, ACT/Exercise, ALD. Users can add/remove sub-forms later. Removing a partially-filled sub-form shows a warning and deletes data on confirmation. Sub-form completion is automatic (when required-to-save fields filled). Overall completion requires all sub-forms complete first, then user-specified.

## Dependencies
- 06-007, 06-008

## Acceptance Criteria
- [ ] 'General Encounter Start' is always included and is the landing page
- [ ] User selects which sub-forms (tabs) to include at creation
- [ ] User can add sub-forms later
- [ ] Removing a partially-filled sub-form warns user and requires confirmation
- [ ] If confirmed, removed sub-form data is deleted
- [ ] Sub-form completion status set automatically when required-to-save fields filled
- [ ] Overall Encounter completion requires all sub-forms complete
- [ ] Overall completion is user-specified (Mark Complete)
- [ ] No duplicate Encounter Date within same program for same patient
- [ ] Sub-form completion status summarized on Patient Dashboard

## Technical Notes
- Tab-based navigation within the Encounter form. Consider a sidebar or horizontal tab bar.
