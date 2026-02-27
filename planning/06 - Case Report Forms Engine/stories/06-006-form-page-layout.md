# Form Page Layout

**Story ID:** 06-006
**Epic:** 06 - Case Report Forms Engine
**Priority:** P0
**SRS Reference:** Section 6.4

## User Story
As a care program user, I want to see consistent patient context and form actions on every case report form page so that I always know whose data I'm editing and can save or complete my work.

## Description
Every CRF page displays the Patient Dashboard header at the top plus essential form context info (Encounter date, Lab date, Care Episode start date, Annual Review year, Phone Note date, Organ). Actions available: Mark form as Complete (where required), Save (finish later), Exit without saving.

## Dependencies
- 05-001 (Patient Dashboard Layout), 06-001 (Form Data Model)

## Acceptance Criteria
- [ ] Patient header displayed on all CRF pages (CFF ID, name, DOB, diagnosis, ALD status)
- [ ] Form-specific context info displayed (encounter date, lab date, etc.)
- [ ] Save button allows saving incomplete data for later
- [ ] Mark Complete button validates completion criteria before setting status
- [ ] Exit without saving discards unsaved changes
- [ ] Unsaved changes prompt if user tries to navigate away

## Technical Notes
- Build as a reusable CRF page wrapper component that all form pages inherit from.
