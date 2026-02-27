# Patient Search — Foundation Admin

**Story ID:** 05-003
**Epic:** 05 - Patient Dashboard & Record Management
**Priority:** P0
**SRS Reference:** Section 5.1

## User Story
As a CF Foundation Administrator, I want to search across all patients in the Registry regardless of program so that I can find and manage any patient record.

## Description
A Patient Search utility exclusive to Foundation Admins. Searches across all patients independent of program membership, including ORH patients regardless of removal reason. Filter by: CFF Program ID, CFF Registry ID, first name, last name, status in programs. Results displayed in table: CFF Registry ID, first/middle/last name, DOB, sex at birth, all care programs associated, vital status (derived from date of death), Last Modified By/Date. Each row links to Patient Dashboard.

## Dependencies
- 04-001 (Patient Data Model)
- 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] Tool accessible only to Foundation Administrators
- [ ] Search across all patients regardless of program
- [ ] Includes ORH patients regardless of removal reason
- [ ] Filters: Program ID, Registry ID, first name, last name, status
- [ ] Results columns: CFF ID, first/middle/last name, DOB, sex at birth, all programs, vital status, last modified by/date
- [ ] Each row links to Patient Dashboard
- [ ] Pagination, search, sort per global list standards

## Technical Notes
- This is a separate tool from the Program Roster search (which is scoped to one program)
- "All programs" column should list program IDs/names for each patient
- Performance consideration: this query spans all patients, needs efficient indexing
