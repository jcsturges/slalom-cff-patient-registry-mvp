# Patient Identity Form

**Story ID:** 04-004
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P0
**SRS Reference:** Section 4.3.1.1

## User Story
As a care program editor, I want to add a new patient by filling out an identity form with consent attestation so that the patient is enrolled in the Registry with proper consent tracking.

## Description
The "Add Patient to Program" workflow starts from the Program Roster. The Patient Identity Form captures: First Name (required), Middle Name (optional), Last Name (required), Last name at birth (optional), Date of Birth (required), Biological sex at birth (required), Last four digits of SSN (optional), Registry ID (optional, if known), Consent attestation checkbox (required, default unchecked): "I acknowledge that the patient has consented to participate in the CF Foundation Patient Registry Study."

The "Continue" button is disabled by default and enabled only when: consent is checked AND the user provides either (first name + last name + DOB + sex) OR (Registry ID + one of first name/last name/DOB).

## Dependencies
- 04-001 (Patient Data Model)
- 04-002 (Patient-Program Association)

## Acceptance Criteria
- [ ] Form captures all specified fields with correct required/optional designation
- [ ] Consent checkbox is unchecked by default
- [ ] "Continue" button disabled until consent checked AND minimum identity fields provided
- [ ] Two valid field combinations: (first+last+DOB+sex) OR (Registry ID + one of first/last/DOB)
- [ ] Upon clicking Continue, system creates provisional add request and triggers duplicate detection
- [ ] Form is accessible from Program Roster and/or Program Selection context

## Technical Notes
- Replace current simplified PatientFormPage with the full identity form
- Consider form validation library (React Hook Form, Formik) for complex conditional rules
- Consent attestation text must be exactly as specified in SRS
