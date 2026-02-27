# Epic 05: Patient Dashboard & Record Management

## Overview

This epic covers the Patient Dashboard (the central hub for viewing and managing a single patient's clinical data), the Foundation Administrator Patient Search tool, program association management by Foundation Admins, hard-delete functionality, and the patient file upload/management system.

The Patient Dashboard is the primary interface from which users access all case report forms for a patient. It displays a header with key patient demographics and a series of tables — one per form type — showing existing forms with status indicators, modification history, and action buttons. Foundation Administrators have additional tools for global patient search, bulk program association changes, and hard-delete capabilities.

## Outcomes

- Patient Dashboard displays patient header and all 10 form type tables in correct order
- Auto-generated empty shells for Demographics, Diagnosis, and Sweat Test forms
- Per-form-type columns (encounter dates, sub-form status dots, organ, etc.) render correctly
- Foundation Admin Patient Search provides global search across all patients including ORH
- Foundation Admins can modify program associations (add, remove, transfer) with audit trail
- Hard-delete permanently erases patient record with CFF ID confirmation
- File upload supports approved types with metadata, auto-renaming, and size validation
- File management supports view/download/edit/delete with program-based permissions

## Key SRS References

- Section 6.3 — Patient Dashboard
- Section 5.1 — Patient Search (Foundation Admin)
- Section 5.1.1 — Modify CF Care Program Association
- Section 5.1.2 — Hard-delete patient record
- Section 6.7 — Patient documents (file upload)

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 05-001 | [Patient Dashboard Layout](stories/05-001-patient-dashboard-layout.md) | P0 | 04-001 |
| 05-002 | [Dashboard Form Tables](stories/05-002-patient-dashboard-form-tables.md) | P0 | 05-001, 06-001 |
| 05-003 | [Patient Search — Admin](stories/05-003-patient-search-admin.md) | P0 | 04-001, 01-004 |
| 05-004 | [Modify Program Association — Admin](stories/05-004-modify-program-association-admin.md) | P1 | 05-003, 04-002 |
| 05-005 | [Hard-Delete Patient](stories/05-005-hard-delete-patient.md) | P1 | 05-003 |
| 05-006 | [Patient File Upload](stories/05-006-patient-file-upload.md) | P1 | 05-001 |
| 05-007 | [Patient File Management](stories/05-007-patient-file-management.md) | P1 | 05-006 |
