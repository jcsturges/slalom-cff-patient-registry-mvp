# Epic 08: Reporting & Report Builder

## Overview

This epic covers the complete reporting infrastructure: the Report Builder (a dynamic query tool replacing portCF ReportWriter), pre-defined data entry and patient list reports for care program users, administrative reports for Foundation Administrators, audit reports, and the global reporting framework (display, download, access control).

The Report Builder is a sophisticated visual query builder that allows users to construct include/exclude patient selection criteria using any data field, customize report rows and columns, save/share queries, and export results. Pre-defined reports cover critical operational needs: incomplete records tracking, patient visit scheduling, diabetes testing monitoring, and administrative oversight.

## Outcomes

- Report Builder supports complex include/exclude queries with AND/AND NOT logic
- All field types supported as filter criteria with appropriate operators
- Report customization allows row-level selection, date filtering, sub-form filtering, column selection
- Report management supports My Reports / My Program's Reports / Global Reports with correct permissions
- All pre-defined reports render correctly with specified columns, filters, and defaults
- Reports enforce program-level data access (CP users see only their program's data)
- CSV/Excel download with header row, all downloads logged
- Foundation Admin reports provide administrative and audit oversight

## Key SRS References

- Section 7 — Reporting Interface (all subsections)
- Section 7.2 — Global reporting requirements
- Section 7.3 — Dynamic reporting (Report Builder)
- Section 7.4 — Data Entry Reports
- Section 7.5 — Patient Lists
- Section 7.6 — Administrative reports
- Section 7.7 — Audit reports

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 08-001 | [Reporting Interface Shell](stories/08-001-reporting-interface-shell.md) | P0 | 03-003, 03-010 |
| 08-002 | [Report Builder — Patient Selection](stories/08-002-report-builder-patient-selection.md) | P0 | 08-001, 06-001 |
| 08-003 | [Report Builder — Customization](stories/08-003-report-builder-customization.md) | P0 | 08-002 |
| 08-004 | [Report Builder — Management](stories/08-004-report-builder-management.md) | P0 | 08-003 |
| 08-005 | [Report Results Display](stories/08-005-report-results-display.md) | P0 | 08-003, 03-010 |
| 08-006 | [Report Download](stories/08-006-report-download.md) | P0 | 08-005 |
| 08-007 | [Incomplete Records Report](stories/08-007-incomplete-records-report.md) | P0 | 06-002 |
| 08-008 | [Patients Due Visit Report](stories/08-008-patients-due-visit-report.md) | P1 | 06-001 |
| 08-009 | [Diabetes Testing Report](stories/08-009-diabetes-testing-report.md) | P1 | 06-001 |
| 08-010 | [Administrative Reports](stories/08-010-admin-reports.md) | P1 | 01-004 |
| 08-011 | [Audit Reports](stories/08-011-audit-reports.md) | P1 | 12-001 |
