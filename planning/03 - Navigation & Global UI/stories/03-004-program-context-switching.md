# Program Context Switching

**Story ID:** 03-004
**Epic:** 03 - Navigation & Global UI
**Priority:** P0
**SRS Reference:** Sections 3.4.3, 3.4.4, 3.7.3

## User Story
As a user assigned to multiple care programs, I want to switch between programs via a dropdown in the header so that I can work with different programs without logging out.

## Description
Users in multiple programs can switch which program they are viewing via a dropdown in the header. Switching programs redirects to the Program Roster for the newly selected program. The system remembers the user's last-selected program for future logins. When a program is selected, all data, reports, and functionality are filtered to that program's context with the user's role for that program.

## Dependencies
- 02-007 (Program-User Association)
- 03-001 (Global Header)

## Acceptance Criteria
- [ ] Dropdown in header shows all programs the user is associated with
- [ ] Selecting a different program redirects to that program's Program Roster
- [ ] All data/reports filter to the selected program context
- [ ] User's role-based permissions change to match their role in the newly selected program
- [ ] System remembers last-selected program across sessions
- [ ] Care program name and ID are displayed in the header after selection

## Technical Notes
- Store last-selected program in user preferences (server-side) or localStorage (client-side)
- Program context should be propagated via React context or similar state management
- API calls should include program context (program ID) for data scoping
