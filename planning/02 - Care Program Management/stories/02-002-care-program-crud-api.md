# Care Program CRUD API

**Story ID:** 02-002
**Epic:** 02 - Care Program Management
**Priority:** P0
**SRS Reference:** Sections 3.8.1.1, 3.8.1.3, 5.2.2

## User Story
As a Foundation Administrator, I want API endpoints for creating, reading, updating, and searching care programs so that the management UI can be built on a reliable backend.

## Description
Build RESTful API endpoints for care program management, restricted to Foundation Administrators:
- `POST /api/programs` — Create a new care program
- `GET /api/programs` — List/search programs (filter by ID, name, city, state)
- `GET /api/programs/{id}` — Get a single program with details
- `PUT /api/programs/{id}` — Update program metadata (name, address, city, state, ZIP, active status)
- Program ID cannot be updated via PUT
- Programs cannot be deleted (no DELETE endpoint)

## Dependencies
- 02-001 (Care Program Data Model)

## Acceptance Criteria
- [ ] POST creates a program with all required fields; rejects duplicate Program IDs
- [ ] GET list supports filtering by Program ID, name, city, state with pagination
- [ ] GET single returns full program details including computed display title
- [ ] PUT updates allowed fields (name, address, city, state, ZIP, active status)
- [ ] PUT rejects attempts to modify Program ID
- [ ] No DELETE endpoint exists; attempting DELETE returns 405
- [ ] All endpoints require Foundation Administrator role
- [ ] Deactivation (setting Active Status to Inactive) is supported via PUT
- [ ] All changes generate audit log entries (old → new values, timestamp, acting user)

## Technical Notes
- Replace or extend current `CareProgram` controller and service
- Ensure Program ID is validated as unique at both application and database level
- Consider optimistic concurrency control for updates
