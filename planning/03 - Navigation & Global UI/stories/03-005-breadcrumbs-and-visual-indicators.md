# Breadcrumbs & Visual Indicators

**Story ID:** 03-005
**Epic:** 03 - Navigation & Global UI
**Priority:** P1
**SRS Reference:** Sections 3.4.5, 3.4.6

## User Story
As a user navigating deep into the Registry, I want breadcrumbs and responsive layout adjustments so that I always know my location and can navigate efficiently.

## Description
Breadcrumb trail displayed for pages 3+ levels deep (e.g., Patients > Program Roster > Patient Details). Current page/section highlighted in menu. Responsive design: Desktop (≥1366px) full horizontal menu with dropdowns, Tablet (768-1365px) collapsible hamburger menu, Mobile (not supported) display message to use desktop.

## Dependencies
- 03-003 (Role-Based Navigation)

## Acceptance Criteria
- [ ] Breadcrumb trail appears for pages 3+ levels deep
- [ ] Current page is highlighted in the navigation menu
- [ ] Desktop (≥1366px): full horizontal menu with dropdowns
- [ ] Tablet (768-1365px): collapsible hamburger menu
- [ ] Mobile (<768px): message indicating desktop is required
- [ ] Breadcrumbs are clickable links to parent pages

## Technical Notes
- Use React Router's location to auto-generate breadcrumbs
- Consider a breadcrumb configuration map for custom labels
