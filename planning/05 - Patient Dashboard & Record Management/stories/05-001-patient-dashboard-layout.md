# Patient Dashboard Layout

**Story ID:** 05-001
**Epic:** 05 - Patient Dashboard & Record Management
**Priority:** P0
**SRS Reference:** Section 6.3

## User Story
As a care program user, I want a Patient Dashboard that shows a patient's clinical summary and all their case report forms so that I can quickly assess the patient's status and navigate to specific forms.

## Description
The Patient Dashboard is the central hub for a single patient. Header section shows: CFF ID, First Name, Last Name, Date of Birth, Most Recent Diagnosis, ALD Status, and "Vital status: Deceased" if applicable. The main section contains tables for each form type in order: Shared Forms (Demographics, Diagnosis, Sweat Test & Fecal Elastase — auto-generated empty shells), Transplants, Annual Review, Encounters, Labs and Tests, Care Episodes, Phone Notes, ALD Status, Files. Each table is paginated at 5 forms per page, sorted in reverse chronological order. Tables are NOT subject to the global search/sort functionality.

## Dependencies
- 04-001 (Patient Data Model)

## Acceptance Criteria
- [ ] Header displays: CFF ID, First Name, Last Name, DOB, Most Recent Diagnosis, ALD Status
- [ ] "Vital status: Deceased" shown if date of death is reported
- [ ] Form tables displayed in correct order (Shared Forms first, then program-specific)
- [ ] Demographics, Diagnosis, Sweat Test auto-generated as empty shells
- [ ] Each table paginated at 5 forms per page
- [ ] Tables sorted in reverse chronological order
- [ ] No search or sort functionality on dashboard tables
- [ ] "Add New" button available for repeatable form types (with role check)

## Technical Notes
- Replace current PatientDetailPage with the SRS-specified dashboard layout
- Auto-generated empty shells: create form records with Incomplete status if they don't exist
- Consider a dashboard API endpoint that returns all form metadata in one call
