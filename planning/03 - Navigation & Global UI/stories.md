# Epic 03: Navigation & Global UI

## Overview

This epic establishes the global user interface shell, navigation structure, and cross-cutting UI components that every page in the Registry depends upon. It includes the global header, role-based navigation menus, program context switching, the help system, announcements, and reusable list components (pagination, search, sort).

The UI must serve two fundamentally different user populations — Care Program users who work within a specific program context and Foundation Administrators who have global, cross-program access. Navigation menus are different for each population, and unauthorized menu items are hidden (not just disabled) at the navigation level. The system must be responsive (desktop-first with tablet read-only support) and meet WCAG 2.1 AA accessibility standards.

## Outcomes

- Global header displays correctly for unauthenticated and authenticated states
- Role-based navigation menus render correctly for CP users and Foundation Admins
- Program context switching works seamlessly with system remembering last-selected program
- Help system provides rich-text help pages in modal overlays with context-sensitive access
- Contact Us form stores requests securely and sends notifications
- Foundation Admins can manage announcements with WYSIWYG editor and scheduling
- Foundation Admins can manage help pages with file/video attachments
- Reusable list components (pagination, search, sort) available for all list views
- WCAG 2.1 AA accessibility compliance across all workflows
- Responsive design: desktop full menu, tablet hamburger, mobile unsupported message

## Key SRS References

- Section 3.1 — Global Header
- Section 3.2 — Home Page (Unauthenticated)
- Section 3.3 — Primary Navigation Structure
- Section 3.4 — Navigation Behavior Requirements
- Section 3.5 — Navigation Implementation Notes
- Section 3.6 — Site Map
- Section 3.7.1 — Help Interface and Contact Us
- Section 3.8.5 — Announcement Manager
- Section 3.8.6 — Help Page Manager
- Section 3.9 — List Functionality (Pagination, Search, Sort)
- Section 1.6 — Usability & Accessibility
- Section 1.7 — Browser and Device Support

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 03-001 | [Global Header](stories/03-001-global-header.md) | P0 | 01-001 |
| 03-002 | [Unauthenticated Home Page](stories/03-002-unauthenticated-home-page.md) | P0 | 03-001 |
| 03-003 | [Role-Based Navigation](stories/03-003-role-based-navigation.md) | P0 | 01-004, 03-001 |
| 03-004 | [Program Context Switching](stories/03-004-program-context-switching.md) | P0 | 02-007, 03-001 |
| 03-005 | [Breadcrumbs & Visual Indicators](stories/03-005-breadcrumbs-and-visual-indicators.md) | P1 | 03-003 |
| 03-006 | [Help System](stories/03-006-help-system.md) | P1 | 03-003 |
| 03-007 | [Contact Us](stories/03-007-contact-us.md) | P1 | 01-001 |
| 03-008 | [Announcement Manager](stories/03-008-announcement-manager.md) | P1 | 01-004 |
| 03-009 | [Help Page Manager](stories/03-009-help-page-manager.md) | P1 | 01-004 |
| 03-010 | [Global List Functionality](stories/03-010-global-list-functionality.md) | P0 | None |
| 03-011 | [Accessibility & Browser Support](stories/03-011-accessibility-and-browser-support.md) | P1 | None |
