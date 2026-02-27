# Help Page Manager

**Story ID:** 03-009
**Epic:** 03 - Navigation & Global UI
**Priority:** P1
**SRS Reference:** Section 3.8.6

## User Story
As a CF Foundation Administrator, I want to create, preview, publish, and manage help pages with file attachments so that users have access to up-to-date documentation.

## Description
Foundation Admins manage help pages via HTML or markdown editor. Features: create/preview/publish/unpublish help pages, organize in hierarchical menu (≥1 level), attach files (≤50MB, videos ≤300MB), draft/unpublished pages visible only to Foundation Admins.

## Dependencies
- 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] Foundation Admins can create help pages via HTML or markdown editor
- [ ] Preview functionality before publishing
- [ ] Publish/unpublish workflow with draft status
- [ ] Draft/unpublished pages visible only to Foundation Admins
- [ ] Help menu organized hierarchically with at least one level
- [ ] File attachments supported (≤50MB per file, videos ≤300MB)
- [ ] Attached files downloadable by all users with access to the help page
- [ ] Help pages support embedded images and videos

## Technical Notes
- Store help content in a hierarchical content table (parent_id for nesting)
- File attachments stored in Azure Blob Storage
- Consider versioning for help page content
