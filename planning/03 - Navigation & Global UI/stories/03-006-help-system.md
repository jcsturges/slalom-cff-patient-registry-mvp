# Help System

**Story ID:** 03-006
**Epic:** 03 - Navigation & Global UI
**Priority:** P1
**SRS Reference:** Section 3.7.1.1

## User Story
As a user, I want access to context-sensitive help pages that open in a modal overlay so that I can get assistance without leaving my current page.

## Description
Help pages contain rich text (headings, lists, bold/italic/underline/strikethrough, links, images PNG/JPEG/GIF, videos MP4). Help pages open in a modal overlay (not a separate tab). A context-sensitive help icon (?) is available on all pages. The Help menu hierarchy has at least one level of depth. Help pages are linked from User Management, Reporting, and each case report form via a configurable "Help" button.

## Dependencies
- 03-003 (Role-Based Navigation)

## Acceptance Criteria
- [ ] Help pages render rich text content (headings, lists, formatting, links, images, videos)
- [ ] Help pages open in a modal overlay, not a new browser tab
- [ ] Context-sensitive help icon (?) visible on all pages
- [ ] Clicking the help icon opens the help page relevant to the current context
- [ ] Help menu hierarchy with at least one level of nesting
- [ ] Help pages support embedded images (PNG, JPEG, GIF) and videos (MP4)
- [ ] Only published help pages are visible to non-admin users

## Technical Notes
- Help content stored in database, managed by Foundation Admins (see 03-009)
- Help page associations (which page links to which help topic) should be configurable
- Consider a help page router that maps URL patterns to help topics
