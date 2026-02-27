# Patient File Management

**Story ID:** 05-007
**Epic:** 05 - Patient Dashboard & Record Management
**Priority:** P1
**SRS Reference:** Section 6.7

## User Story
As a care program user, I want to view, download, and manage patient files with appropriate permissions so that I can access clinical documents while respecting data ownership.

## Description
File list on Patient Dashboard paginated at 5 files per page, showing file type, date uploaded, and care program. CP Admins and Editors can view, edit metadata, and delete files uploaded by their program. All users can view and download files uploaded by other programs or Foundation Admins, but cannot delete or modify them.

## Dependencies
- 05-006 (Patient File Upload)

## Acceptance Criteria
- [ ] File list paginated at 5 per page
- [ ] Files display: file type, date uploaded, care program
- [ ] CP Admins/Editors: view, download, edit metadata, delete files from their program
- [ ] All users: view and download files from other programs (no edit/delete)
- [ ] Foundation Admins: full access to all files
- [ ] Delete requires confirmation
- [ ] File download triggers download event logging

## Technical Notes
- File access permissions are program-based, not user-based
- Download URLs should be time-limited signed URLs (Azure Blob SAS tokens)
- Consider inline preview for images and PDFs
