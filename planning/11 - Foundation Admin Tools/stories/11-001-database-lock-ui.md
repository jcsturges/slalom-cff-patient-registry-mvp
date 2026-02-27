# Database Lock UI

**Story ID:** 11-001
**Epic:** 11 - Foundation Admin Tools
**Priority:** P0
**SRS Reference:** Sections 3.8.7, 6.2.3

## User Story

As a Foundation Admin, I want to configure and initiate the annual database lock from a dedicated admin interface so that I can control when the preceding reporting year's data becomes read-only across the registry.

## Description

The Database Lock UI is the Foundation Admin's primary interface for managing the annual lock process. The screen allows the admin to select the reporting year to lock, set the exact lock date, and choose between two execution modes (fast synchronous or overnight batch). The UI must clearly communicate the scope and consequences of the lock operation before execution, including the number of affected forms and active sessions.

The interface is accessible only to users with the **FoundationAdmin** role. It lives under the Foundation Admin section of the navigation (e.g., `/admin/database-lock`). The page displays the current lock status, history of prior locks, and a form for configuring the next lock.

### Key UI Elements

1. **Lock Configuration Form:**
   - Reporting year selector (dropdown of years, defaulting to the preceding calendar year)
   - Lock date picker (must be a future date; defaults to a CFF-configured standard date if available)
   - Execution mode radio group: "Fast Synchronous (immediate, <2 minutes)" or "Overnight Batch (scheduled, 2-6 AM ET)"
   - For overnight batch: date picker for the scheduled execution night
   - Pre-lock impact summary: count of forms that will be locked, count of currently active sessions on affected forms

2. **Confirmation Dialog:**
   - Summarizes: reporting year, lock date, execution mode, affected form count
   - Requires explicit acknowledgment ("I understand this action will lock [N] forms for reporting year [YYYY]")
   - "Confirm & Execute" / "Cancel" buttons

3. **Lock Status Dashboard:**
   - Current lock state: Unlocked / Pending / In Progress / Completed / Failed
   - For in-progress locks: progress bar with form count processed vs total
   - History table: past lock operations with year, date, mode, status, initiated by, completed at

4. **Post-Lock Status:**
   - After successful lock, display summary: forms locked, forms skipped (in-progress), timestamp
   - Link to view skipped forms (forms that were in active edit sessions at lock time)

## Dependencies

- **Epic 01:** FoundationAdmin role and Okta RBAC must be configured.
- **Epic 06:** Form engine must expose an API to count forms by reporting year and status.
- **Story 11-002:** Lock execution engine (this story covers UI only; 11-002 covers the backend).
- **Epic 03:** Foundation Admin navigation section must exist.

## Acceptance Criteria

- [ ] The Database Lock page is accessible at `/admin/database-lock` and restricted to FoundationAdmin role only.
- [ ] Non-FoundationAdmin users who navigate directly to `/admin/database-lock` see an `<Alert severity="error">` stating the required role.
- [ ] The reporting year dropdown lists available years and defaults to the preceding calendar year.
- [ ] The lock date picker only allows future dates.
- [ ] The execution mode selector offers "Fast Synchronous" and "Overnight Batch" options with clear descriptions.
- [ ] Selecting "Overnight Batch" reveals a date picker for scheduling the execution window.
- [ ] Before execution, a confirmation dialog displays the reporting year, lock date, execution mode, and affected form count.
- [ ] The confirmation dialog requires an explicit acknowledgment checkbox before the "Confirm & Execute" button is enabled.
- [ ] The lock status dashboard shows the current lock state with real-time progress for in-progress operations.
- [ ] The lock history table displays all prior lock operations with sortable columns.
- [ ] After a successful lock, a summary shows forms locked, forms skipped, and completion timestamp.
- [ ] The "Confirm & Execute" button is disabled (with tooltip) if a lock is already in progress for the selected year.
- [ ] All UI actions on this page are audit-logged (page view, configuration changes, execution initiation).
- [ ] The page is fully responsive and follows MUI v5 design patterns consistent with the rest of the application.

## Technical Notes

### Frontend

- **Route:** `/admin/database-lock` — add to the Foundation Admin route group in the React Router configuration.
- **Component:** `DatabaseLockPage.tsx` in `src/ngr-web-app/src/pages/admin/`.
- **Role gate:** Use `useRoles` hook to check for `FoundationAdmin`. Render `<Alert>` for unauthorized access (per RBAC UI Convention — never hide, always disable/explain).
- **API calls:**
  - `GET /api/admin/database-locks` — list lock history and current status.
  - `GET /api/admin/database-locks/impact?reportingYear={year}` — get count of forms that would be affected.
  - `POST /api/admin/database-locks` — initiate a lock (body: `{ reportingYear, lockDate, executionMode, scheduledDate? }`).
  - `GET /api/admin/database-locks/{id}/progress` — poll for in-progress lock status (or use SignalR for real-time updates).
- **State management:** React Query for server state; local state for form inputs.
- **Polling:** For in-progress locks, poll the progress endpoint every 5 seconds (or integrate SignalR hub for push updates).

### Backend API

- **Controller:** `DatabaseLockController.cs` in `NgrApi.Controllers.Admin` namespace.
- **Authorization:** `[Authorize(Policy = "FoundationAdmin")]` on all endpoints.
- **DTOs:** `DatabaseLockConfigDto`, `DatabaseLockStatusDto`, `DatabaseLockImpactDto`.

### Database

- **Table:** `DatabaseLocks` — stores lock configuration and status.
  - Columns: `Id`, `ReportingYear`, `LockDate`, `ExecutionMode` (enum: Synchronous/Batch), `ScheduledDate`, `Status` (enum: Pending/InProgress/Completed/Failed), `InitiatedBy`, `InitiatedAt`, `CompletedAt`, `FormsLocked`, `FormsSkipped`, `ErrorMessage`.
- **Constraint:** Unique index on `ReportingYear` — only one lock operation per year (re-runs create new records).
