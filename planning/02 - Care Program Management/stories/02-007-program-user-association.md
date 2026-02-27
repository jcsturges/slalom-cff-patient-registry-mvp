# Program-User Association

**Story ID:** 02-007
**Epic:** 02 - Care Program Management
**Priority:** P0
**SRS Reference:** Sections 2.1, 2.3.3, 3.7.3

## User Story
As a system architect, I want to model user-program associations with per-program roles so that users can have different levels of access across different care programs.

## Description
Implement the data model for user-program associations. A user can be associated with one or more care programs, each with an independently assigned role. The association record is the authoritative source for: which programs a user can access, what role they hold in each program, and their active/deactivated status per program.

When a user with one program association logs in, they bypass program selection and go directly to that program's roster. Users with multiple associations see the Program Selection page and can switch between programs via the header dropdown.

## Dependencies
- 02-001 (Care Program Data Model)
- 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] User-program association model supports: user_id, program_id, role, status (active/deactivated)
- [ ] A user can have different roles across different programs
- [ ] Single-program users bypass Program Selection and land directly on Program Roster
- [ ] Multi-program users see the Program Selection page with their programs listed
- [ ] Program Selection displays: Program Name, Program Type, User Role
- [ ] System remembers the user's last-selected program for future logins
- [ ] When a user selects a program, all functionality aligns with their role in that program
- [ ] Association changes (add, change role, deactivate) are logged

## Technical Notes
- Table structure: `user_program_associations(id, user_id, program_id, role, status, created_at, updated_at, deactivated_at)`
- The "last selected program" preference can be stored in a user preferences table or local storage
- Program Selection page should include the mockup layout from SRS Section 3.7.3 (table with Program Name, Program Type, User Role)
