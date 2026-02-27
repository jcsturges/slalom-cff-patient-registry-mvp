# Accessibility & Browser Support

**Story ID:** 03-011
**Epic:** 03 - Navigation & Global UI
**Priority:** P1
**SRS Reference:** Sections 1.6, 1.7

## User Story
As a user with accessibility needs, I want the Registry to conform to WCAG 2.1 AA standards and work across major browsers so that I can use the system effectively regardless of my abilities or browser choice.

## Description
The Registry must conform to WCAG 2.1 AA accessibility standards including: keyboard navigation for all key workflows (patient search, roster navigation, case report forms, report builder), tab navigation for all elements, skip navigation link for screen readers, ARIA labels on all menu items, and sufficient color contrast. Supported browsers: Chrome, Edge, Safari (current and previous major version). Minimum resolution: 1024x768. Read-only views display correctly on tablets (768px width). No horizontal scrolling required.

## Dependencies
- None (cross-cutting requirement)

## Acceptance Criteria
- [ ] Key workflows are fully usable via keyboard only
- [ ] All navigation elements support Tab navigation
- [ ] Skip navigation link provided for screen readers
- [ ] ARIA labels on all menu items and interactive elements
- [ ] Color contrast meets WCAG 2.1 AA requirements
- [ ] Supported browsers: Chrome, Edge, Safari (current + previous major version)
- [ ] Degradation on unsupported browsers handled with clear message
- [ ] Minimum resolution 1024x768 with no horizontal scrolling
- [ ] Read-only views display correctly on tablets (768px width)
- [ ] All interactive elements accessible via touch on tablets

## Technical Notes
- Use axe-core or similar tool for automated accessibility testing
- Consider Playwright accessibility assertions in E2E tests
- MUI components provide good baseline ARIA support but need custom labels for domain-specific elements
