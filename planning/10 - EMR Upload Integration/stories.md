# Epic 10: EMR Upload Integration

## Overview

This epic covers the integration mechanism for importing Electronic Medical Record (EMR) data from CF care programs into the Registry. EMR data pre-populates Demographics, Encounter, and Labs and Tests case report forms for patients with established Registry records. Each institution maintains a crosswalk between their EMR medical record number (MRN) and the CFF Registry ID.

Two transfer mechanisms are supported: manual CSV file upload by care program users and automated SFTP transfer at defined intervals. The CSV format includes approximately 240 fields. EMR-driven updates trigger special form status behavior (automatic downgrade to Incomplete with a review banner).

## Outcomes

- Care program admins and editors can manually upload CSV files via the UI
- SFTP automated transfer endpoint supports scheduled imports
- CSV field mapping correctly populates Demographics, Encounter, and Labs forms
- File-level and field-level validation with hierarchical error checking
- EMR-driven form updates correctly trigger Incomplete status with review banner
- System allows adding new CSV fields without breaking existing connections

## Key SRS References

- Section 9 — EMR Upload
- Section 6.2.1.2 — Status behavior on EMR-initiated updates

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 10-001 | [CSV Upload Interface](stories/10-001-csv-upload-interface.md) | P1 | 06-001, 01-004 |
| 10-002 | [SFTP Automated Transfer](stories/10-002-sftp-automated-transfer.md) | P2 | 10-001 |
| 10-003 | [EMR Field Mapping](stories/10-003-emr-field-mapping.md) | P1 | 06-001, 06-007 |
| 10-004 | [EMR Import Validation](stories/10-004-emr-import-validation.md) | P1 | 10-003 |
| 10-005 | [EMR Status Integration](stories/10-005-emr-status-integration.md) | P1 | 06-002, 06-012 |
