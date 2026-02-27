# Program Roster View

**Story ID:** 04-003
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P0
**SRS Reference:** Section 4.3

## User Story
As a care program user, I want to view a searchable list of all patients in my program so that I can find patients and navigate to their records.

## Description
The Program Roster is the primary list view displayed when a care program user selects their program. It shows all patients associated with the selected program with columns: CFF ID (hyperlinked to Patient Dashboard), First name, Last name, Date of Birth, Biological sex at birth, Diagnosis, List of other CF care programs, Vital status (Alive or Deceased), Last user to modify/add data and date. Supports search, sort, and pagination per global standards.

## Dependencies
- 04-002 (Patient-Program Association)
- 03-010 (Global List Functionality)

## Acceptance Criteria
- [ ] Roster displays all patients with Active association to the selected program
- [ ] Columns: CFF ID (hyperlink), First name, Last name, DOB, Sex at birth, Diagnosis, Other programs, Vital status, Last modified by/date
- [ ] CFF ID links to Patient Dashboard
- [ ] Search, sort, and pagination per global list standards (03-010)
- [ ] Default sort by Last Name, then First Name
- [ ] "Add Patient" action available for CP Admins and Editors
- [ ] "Remove from Program" action available per patient

## Technical Notes
- Replace current PatientListPage with the SRS-specified roster layout
- "Other programs" column shows names of other programs (excluding current), not IDs
- Consider lazy-loading the "Other programs" column if it impacts performance
