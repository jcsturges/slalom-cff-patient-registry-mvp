# File Migration

**Story ID:** 13-007
**Epic:** 13 - Data Feed & Migration
**Priority:** P1
**SRS Reference:** Section 11

## User Story
As a system architect, I want to migrate all patient files from portCF to HIPAA-compliant storage so that existing clinical documents are accessible in the new system.

## Description
Existing clinical documents are accessible in the new system

## Dependencies
- 05-006

## Acceptance Criteria
- [ ] All previously uploaded portCF files migrated to HIPAA-compliant storage
- [ ] File metadata preserved and migrated with each file
- [ ] Files appear in NGR as if uploaded natively
- [ ] Users can view, download, edit metadata, and delete migrated files per normal permissions
- [ ] Files excluded for patients with date of death (for file uploads only)

## Technical Notes
- File migration volume may be significant — consider batch processing with progress tracking.
