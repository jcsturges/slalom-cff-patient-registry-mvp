# Epic 06: Case Report Forms Engine

## Overview

This epic covers the generic form engine that powers all 10 case report form (CRF) types in the Registry. It includes the data model for form instances, the dual-status system (Complete/Incomplete + Unlocked/Locked), form deletion rules, the annual database lock mechanism, the form page layout with patient context, all supported field types, repeat blocks, calculated fields, the multi-tier validation engine, field dependencies, and EMR-driven status behavior.

The form engine must be flexible enough to render 10 different form types (each with unique field specifications defined in external documents), while enforcing consistent behavior for saving, completion, locking, deletion, and error handling across all forms. The engine supports both shared forms (visible across programs) and program-specific forms (editable only by the owning program).

## Outcomes

- Data model supports 10 CRF types with shared vs program-specific classification
- Dual status system (Complete/Incomplete + Unlocked/Locked) with independent transitions
- Form deletion with role-based permissions, permanent removal, and audit trail
- Database lock execution locks forms by reporting year without disrupting active sessions
- Form page layout consistently displays patient header, context info, and form actions
- All 9 field types supported: text, numeric, date, radio, checkbox, dropdown, repeat blocks, multi-select, multi-line
- Repeat blocks with collapsible UI, add/edit/delete, and table summary
- Calculated fields (FEV1, percentiles, age, vital status) update within performance requirements
- 4-tier validation engine: warnings, completion-blocking, save-blocking errors
- Field dependency warnings prevent silent data loss
- EMR-driven updates correctly downgrade form status with review banner

## Key SRS References

- Section 6.1 — Overview (10 forms, shared vs program-specific)
- Section 6.2 — Form Statuses and Status transitions
- Section 6.2.1 — Complete Status
- Section 6.2.2 — Form Deletion
- Section 6.2.3 — Database Lock and Locking Status
- Section 6.3 — Patient Dashboard
- Section 6.4 — Case report form pages
- Section 6.5 — Case report form functionality
- Section 6.5.1 — Error handling of user-entered information
- Section 6.8 — Case Report Form Specifications (field specs, calculated fields)

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 06-001 | [Form Data Model](stories/06-001-form-data-model.md) | P0 | 04-001 |
| 06-002 | [Form Status Management](stories/06-002-form-status-management.md) | P0 | 06-001 |
| 06-003 | [Form Locking Status](stories/06-003-form-locking-status.md) | P0 | 06-002, 11-001 |
| 06-004 | [Form Deletion](stories/06-004-form-deletion.md) | P0 | 06-001, 01-004 |
| 06-005 | [Database Lock Execution](stories/06-005-database-lock-execution.md) | P0 | 06-003, 11-001 |
| 06-006 | [Form Page Layout](stories/06-006-form-page-layout.md) | P0 | 05-001, 06-001 |
| 06-007 | [Field Type Support](stories/06-007-field-type-support.md) | P0 | 06-006 |
| 06-008 | [Repeat Blocks](stories/06-008-repeat-blocks.md) | P0 | 06-007 |
| 06-009 | [Calculated Fields](stories/06-009-calculated-fields.md) | P1 | 06-007 |
| 06-010 | [Form Validation Engine](stories/06-010-form-validation-engine.md) | P0 | 06-007 |
| 06-011 | [Field Dependencies](stories/06-011-field-dependencies.md) | P0 | 06-010 |
| 06-012 | [EMR Status Downgrade](stories/06-012-emr-status-downgrade.md) | P1 | 06-002, 10-003 |
