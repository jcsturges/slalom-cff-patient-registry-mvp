# Announcement Manager

**Story ID:** 03-008
**Epic:** 03 - Navigation & Global UI
**Priority:** P1
**SRS Reference:** Section 3.8.5

## User Story
As a CF Foundation Administrator, I want to create, edit, schedule, and manage announcements with rich text formatting so that I can communicate important information to all Registry users.

## Description
Foundation Admins manage announcements shown on the main page to all users. Features: WYSIWYG rich text editor (Bold, Italic, Underline, Strikethrough, predefined colors, H2-H4 headings, bulleted/numbered lists, hyperlinks, code blocks, blockquotes), effective date with optional expiration, preview before activation, ≥5,000 character limit, all times in Eastern Time (ET). Auto-deactivation at 11:59 PM ET on expiration date. Deactivated announcements are deleted from the system.

## Dependencies
- 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] Foundation Admins can create announcements with WYSIWYG rich text editor
- [ ] Supported formatting: Bold, Italic, Underline, Strikethrough, predefined colors, H2-H4, lists, links, code blocks, blockquotes
- [ ] Announcements have effective date and optional expiration date
- [ ] Preview functionality before activation
- [ ] Auto-deactivation at 11:59 PM ET on expiration date
- [ ] No expiration → remains active until manually deactivated
- [ ] Zero, one, or multiple active announcements supported simultaneously
- [ ] Active announcements display to all users upon every login
- [ ] Upper size limit ≥5,000 characters
- [ ] All times display as Eastern Time with "All times in Eastern Time" indicator
- [ ] Deactivated announcements are deleted from the system

## Technical Notes
- Consider TipTap, Quill, or Slate for the WYSIWYG editor
- Store announcement content as sanitized HTML
- Use a predefined color palette for brand consistency
