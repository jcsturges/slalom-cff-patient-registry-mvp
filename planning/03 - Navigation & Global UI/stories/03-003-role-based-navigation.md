# Role-Based Navigation

**Story ID:** 03-003
**Epic:** 03 - Navigation & Global UI
**Priority:** P0
**SRS Reference:** Sections 3.3, 3.4.2

## User Story
As an authenticated user, I want to see only the navigation items relevant to my role so that I can easily find the features I have access to.

## Description
Primary navigation menu organized hierarchically based on user role and selected program:

**Care Program Users:** Help, CF Care Program User Management, Program Roster, Reporting, EMR Upload

**CF Foundation Administrators:** Announcements Manager, Help Manager, Database Lock, User Analytics, Care Program Management, Patient Search, User Management

Menu items for unauthorized roles are **not visible** (hidden, not just disabled). This is navigation-level access control, distinct from the action-button RBAC pattern.

## Dependencies
- 01-004 (Role Authorization Model)
- 03-001 (Global Header)

## Acceptance Criteria
- [ ] CP users see only: Help, User Management, Program Roster, Reporting, EMR Upload
- [ ] Foundation Admins see their dedicated menu items
- [ ] Unauthorized menu items are hidden (not visible at all)
- [ ] Active menu item is visually highlighted
- [ ] Menu remains visible on all pages
- [ ] Menu type: horizontal top navigation bar with dropdown menus for sub-items

## Technical Notes
- Navigation menu items should be driven by a configuration that maps roles to visible items
- Consider a `NavigationConfig` that both frontend and backend can reference
- Desktop (≥1366px): full horizontal menu with dropdowns; Tablet (768-1365px): collapsible hamburger
