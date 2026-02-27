# Epic 09: Data Export & Raw Downloads

## Overview

This epic covers the program-level raw data export system that allows authorized care program users to download registry data for their patients in structured CSV format. Users can select specific eCRFs, apply date range and completeness filters, choose between coded and descriptive output formats, and save download parameter sets for reuse.

The system enforces program-level data access restrictions and packages multi-form downloads as ZIP archives with clear file naming conventions.

## Outcomes

- Users can select one or more eCRFs (or "All Forms") for download
- Date range and completeness filters work correctly
- Two output formats: coded values and human-readable descriptives
- Downloads packaged as ZIP with one file per form type
- Saved download definitions support create, reuse, edit, Save As, delete
- Program-level data access enforced (users only download their program's data)
- All downloads logged for audit purposes

## Key SRS References

- Section 8 — Program-level raw data export (eCRF exports)
- Section 8.1 — Selection of data for export
- Section 8.2 — Download format and packaging
- Section 8.2.1 — Saved download definitions

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 09-001 | [Raw Data Download UI](stories/09-001-raw-data-download-ui.md) | P1 | 06-001, 01-004 |
| 09-002 | [Export Format & Packaging](stories/09-002-export-format-packaging.md) | P1 | 09-001 |
| 09-003 | [Saved Download Definitions](stories/09-003-saved-download-definitions.md) | P2 | 09-001 |
