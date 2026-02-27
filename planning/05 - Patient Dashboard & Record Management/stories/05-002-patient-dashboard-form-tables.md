# Dashboard Form Tables

**Story ID:** 05-002
**Epic:** 05 - Patient Dashboard & Record Management
**Priority:** P0
**SRS Reference:** Section 6.3

## User Story
As a care program user, I want each form type table on the Patient Dashboard to show relevant metadata columns so that I can quickly assess form completion status.

## Description
Each form type table includes: form status (Complete/Incomplete indicator), Last Modified Date, Last Modified By, link to each existing form, delete button (trash icon, role-gated). Additional columns vary by form type:
- **Transplant:** Organ, Most Recent Step, Most Recent Step Date
- **Annual Review:** Annual Review Year
- **Encounter:** Encounter Date, status indicator (colored dot) for each sub-form
- **Labs and Tests:** Lab Date, status indicator for each sub-form
- **Care Episode:** Start Date, End Date
- **Phone Note:** Phone Note Date

## Dependencies
- 05-001 (Patient Dashboard Layout)
- 06-001 (Form Data Model)

## Acceptance Criteria
- [ ] All form tables show: status indicator, Last Modified Date/By, form link, delete button
- [ ] Delete button is role-gated (disabled with tooltip for unauthorized users)
- [ ] Transplant table shows Organ, Most Recent Step, Most Recent Step Date
- [ ] Annual Review table shows Year
- [ ] Encounter table shows Encounter Date + colored dot per sub-form status
- [ ] Labs table shows Lab Date + colored dot per sub-form status
- [ ] Care Episode table shows Start Date, End Date
- [ ] Phone Note table shows Phone Note Date
- [ ] Form links navigate to the form data entry page
- [ ] "Add New" links create new form instances (where applicable)

## Technical Notes
- Colored dots for sub-form status: green = complete, red = incomplete, gray = not created
- "Add New" availability depends on form type rules (e.g., only one Demographics per patient)
