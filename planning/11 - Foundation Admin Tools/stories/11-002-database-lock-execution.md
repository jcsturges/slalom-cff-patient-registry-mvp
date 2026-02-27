# Database Lock Execution Engine

**Story ID:** 11-002
**Epic:** 11 - Foundation Admin Tools
**Priority:** P0
**SRS Reference:** Sections 3.8.7, 6.2.3

## User Story

As a Foundation Admin, I want the database lock to reliably lock all eligible forms for the selected reporting year while preserving active editing sessions so that the annual data closure is complete and accurate without disrupting users mid-entry.

## Description

The Database Lock Execution Engine is the backend service that performs the actual form locking when triggered by the Foundation Admin (via story 11-001). It must support two execution modes, handle active sessions gracefully, and enforce post-lock date validation rules across the form engine.

### Execution Modes

**Fast Synchronous (immediate, <2 minutes):**
- Executes within the HTTP request lifecycle (with a generous timeout).
- Iterates through all forms with dates falling within the reporting year.
- Sets `IsLocked = true` and `LockedAt = UTC now` on each eligible form submission.
- Skips forms currently being edited (active session detected via the form engine's session tracking).
- Returns a summary to the caller upon completion.

**Overnight Batch (scheduled, 2-6 AM ET):**
- The lock request is persisted with status `Pending` and a `ScheduledDate`.
- A background job (Hangfire recurring job or Azure Functions timer trigger) picks up pending locks during the 2-6 AM ET window.
- Processes forms in configurable batch sizes (default: 500) with progress tracking.
- Updates the `DatabaseLock` record's progress after each batch.
- On failure, marks the lock as `Failed` with error details and allows retry.

### Active Session Handling

- When the lock engine encounters a form that has an active editing session (user has the form open and unsaved), it **skips** that form.
- Skipped forms are recorded in a `DatabaseLockSkippedForms` table with the form ID, session user, and skip reason.
- When the user saves the skipped form, the form engine checks if a lock is active for the form's reporting year. If so, the form is automatically locked upon save (the "auto-lock on save" behavior).
- A background reconciliation job runs 24 hours after lock completion to catch any forms that were skipped but never saved (orphaned sessions). These are force-locked with an audit note.

### Post-Lock Enforcement

Once a reporting year is locked:
- **Form date validation:** New form entries must have dates in the current (unlocked) year. Attempts to create forms with dates in the locked year are rejected with a clear validation message.
- **Annual Reviews:** All Annual Review forms for the locked year are set to read-only.
- **Encounter/Labs/Episode/Phone forms:** Forms with service dates falling within the locked reporting year are set to read-only.
- **Existing unlocked forms:** The form engine queries the `DatabaseLock` table on every form load to determine if the form's reporting year is locked.

## Dependencies

- **Story 11-001:** Database Lock UI provides the trigger for this engine.
- **Epic 06:** Form engine must support `IsLocked` flag, read-only rendering, and date validation against lock status.
- **Epic 12:** All lock operations must be audit-logged.
- **Hangfire or Azure Functions:** Background job infrastructure must be available for overnight batch mode.

## Acceptance Criteria

- [ ] Fast Synchronous mode locks all eligible forms for the reporting year within 2 minutes for up to 100,000 forms.
- [ ] Overnight Batch mode queues the lock and executes during the 2-6 AM ET window.
- [ ] Overnight Batch processes forms in configurable batch sizes with progress updates after each batch.
- [ ] Forms with active editing sessions are skipped during lock execution (not interrupted).
- [ ] Skipped forms are recorded in the `DatabaseLockSkippedForms` table with form ID, session user, and reason.
- [ ] When a user saves a skipped form after lock execution, the form is automatically locked upon save.
- [ ] A reconciliation job runs 24 hours after lock completion to force-lock orphaned skipped forms.
- [ ] Post-lock, new form entries with dates in the locked reporting year are rejected with a validation error message: "The [YYYY] reporting year has been locked. New entries must use dates in [current year]."
- [ ] Post-lock, Annual Review forms for the locked year render as read-only in the form engine.
- [ ] Post-lock, Encounter, Labs, Care Episode, and Phone Note forms with service dates in the locked year render as read-only.
- [ ] Lock operations are idempotent: re-running a lock for an already-locked year does not cause errors or duplicate records.
- [ ] Failed batch locks can be retried from the point of failure (not from the beginning).
- [ ] All lock operations (start, progress, skip, completion, failure, reconciliation) are audit-logged.
- [ ] Lock status transitions follow: Pending -> InProgress -> Completed/Failed.
- [ ] The lock engine does not hold database-level table locks; it uses row-level operations to avoid blocking other users.

## Technical Notes

### Backend Service

- **Service:** `DatabaseLockService.cs` in `NgrApi.Services.Admin` namespace.
- **Interface:** `IDatabaseLockService` with methods:
  - `Task<DatabaseLockResult> ExecuteSynchronousLockAsync(int reportingYear, DateTime lockDate)`
  - `Task<Guid> ScheduleBatchLockAsync(int reportingYear, DateTime lockDate, DateTime scheduledDate)`
  - `Task<DatabaseLockProgress> GetProgressAsync(Guid lockId)`
  - `Task ReconcileSkippedFormsAsync(Guid lockId)`
- **Batch processing:** Use `IQueryable` with `.Skip().Take()` pagination, not `.ToList()` on the full set.
- **Transaction scope:** Each batch of 500 forms is committed in its own transaction. If a batch fails, prior batches remain committed, and the lock records its progress for retry.

### Form Engine Integration

- **Form load hook:** When the form engine loads a form, it must check `DatabaseLock` table for the form's reporting year. If locked, render as read-only.
- **Form save hook:** On save, if the form was in the skipped list, set `IsLocked = true` and remove from `DatabaseLockSkippedForms`.
- **Date validation middleware:** Add a validation rule in the form engine that rejects new form submissions with dates in a locked reporting year.

### Database Schema

- **Table: `DatabaseLocks`** (from 11-001, extended):
  - Add `ProgressFormsProcessed` (int), `ProgressFormsTotal` (int) for batch tracking.
  - Add `RetryCount` (int, default 0), `LastRetryAt` (datetime, nullable).

- **Table: `DatabaseLockSkippedForms`**:
  - Columns: `Id`, `DatabaseLockId` (FK), `FormSubmissionId`, `SessionUserId`, `SkipReason`, `SkippedAt`, `ResolvedAt` (nullable), `ResolutionType` (enum: AutoLockedOnSave / ForceLockedByReconciliation / null).

### Background Jobs

- **Hangfire job:** `DatabaseLockBatchJob` — registered as a recurring job that checks for `Pending` locks with `ScheduledDate <= now` during the 2-6 AM ET window.
- **Reconciliation job:** `DatabaseLockReconciliationJob` — runs 24 hours after lock completion, queries `DatabaseLockSkippedForms` where `ResolvedAt IS NULL`, and force-locks those forms.

### Performance Considerations

- For Fast Synchronous mode, use `ExecuteSqlRaw` for bulk updates when possible (e.g., `UPDATE FormSubmissions SET IsLocked = 1, LockedAt = @now WHERE ReportingYear = @year AND IsLocked = 0 AND Id NOT IN (SELECT FormId FROM ActiveSessions)`).
- For Overnight Batch mode, the 500-row batch size is configurable via `appsettings.json` (`DatabaseLock:BatchSize`).
- Add a database index on `FormSubmissions(ReportingYear, IsLocked)` for efficient filtering.
