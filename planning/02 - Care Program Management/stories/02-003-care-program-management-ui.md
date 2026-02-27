# Care Program Management UI

**Story ID:** 02-003
**Epic:** 02 - Care Program Management
**Priority:** P0
**SRS Reference:** Sections 3.8.1, 5.2.2

## User Story
As a CF Foundation Administrator, I want a management interface to search for, view, create, edit, and deactivate care programs so that I can maintain the network of CF care centers.

## Description
Build the Care Program Management UI accessible only to Foundation Administrators. Features include:
- **Program List:** Searchable list with columns for ID, name, active status. Each row has a "View/Edit" action.
- **Search:** Filter by Program ID, name, city, state.
- **Create:** Form to add a new program with all metadata fields.
- **Edit:** Open a program in edit mode to update name, address, city, state, ZIP, active status. Program ID is read-only.
- **View Users:** Within a program's view/edit screen, display associated users (ID, name, email, role, status, last login).

## Dependencies
- 02-002 (Care Program CRUD API)
- 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] Program list displays: Program ID, name, active status with search and pagination
- [ ] Search filters: Program ID, name, city, state
- [ ] Create form captures all required metadata; validates Program ID uniqueness
- [ ] Edit form allows updating name, address, city, state, ZIP, active status
- [ ] Edit form displays Program ID as read-only
- [ ] Program cannot be deleted from the UI
- [ ] Users associated with the program are visible (ID, name, email, role, status, last login)
- [ ] Navigation from user row to user detail/edit screen is supported
- [ ] Interface is only accessible to Foundation Administrators

## Technical Notes
- Part of the Foundation Admin navigation menu section
- Consider a split-panel layout: program list on left, details on right
- User list within program context should support navigation to user edit screen (01-006)
