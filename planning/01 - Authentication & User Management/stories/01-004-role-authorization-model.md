# Role Authorization Model

**Story ID:** 01-004
**Epic:** 01 - Authentication & User Management
**Priority:** P0
**SRS Reference:** Sections 2.1, 2.2, User Roles Table 1

## User Story
As a system architect, I want to implement a role-based access control model with four roles and per-program role assignments so that users have appropriate permissions scoped to their care programs.

## Description
Implement the RBAC model with four roles across two categories:

**Care Program Roles** (per-program assignment):
- **CF Care Program Administrator:** Full data access within their program, can manage users, merge records, delete forms
- **CF Care Program Editor:** Can enter/edit data and manage files, but cannot manage users or merge
- **CF Care Program Report User:** Read-only access to data and reports within their program

**Foundation Role** (global):
- **CF Foundation Administrator:** Highest privilege, global access across all programs, can manage users/programs, edit locked forms, impersonate users, hard-delete records

**Mutual exclusion rule:** A user cannot hold both a CF Foundation Administrator role and a CF Care Program role simultaneously.

**Per-program flexibility:** A user can have different care program roles across different programs (e.g., Administrator in Program 10, Editor in Program 104).

## Dependencies
- 01-001 (Okta SAML 2.0 Integration)
- 02-001 (Care Program Data Model)

## Acceptance Criteria
- [ ] Four roles are implemented: CP Administrator, CP Editor, CP Report User, Foundation Administrator
- [ ] Users can be assigned different roles across different care programs
- [ ] Mutual exclusion enforced: cannot have Foundation Admin + any CP role simultaneously
- [ ] API authorization policies enforce role-based access on all endpoints
- [ ] UI reads permissions from the authenticated session and gates actions accordingly
- [ ] Role assignments are stored per user-program association (not globally)
- [ ] Foundation Administrators have implicit access to all programs and patients
- [ ] Permission checks are consistent between API and UI (no client-side-only enforcement)

## Technical Notes
- Refactor current `useRoles` hook and `Program.cs` policies to support the 4-role model (current MVP uses simplified groups)
- User-program-role association table: `user_id, program_id, role, status, created_at, updated_at`
- Foundation Admin role is stored separately (not associated with a program)
- Consider middleware that resolves the user's effective permissions based on their active program context
