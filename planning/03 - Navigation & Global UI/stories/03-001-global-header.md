# Global Header

**Story ID:** 03-001
**Epic:** 03 - Navigation & Global UI
**Priority:** P0
**SRS Reference:** Section 3.1

## User Story
As a user, I want a persistent header on all pages showing my identity and program context so that I always know where I am in the system.

## Description
The global header appears on every page. For unauthenticated users: CFF logo (left-aligned) and "Log In" button (right-aligned). For authenticated users: CFF logo (clickable, links to cff.org), selected care program name and ID (if applicable), care program selector dropdown (for multi-program users), user name displayed as "FirstName LastName", and "Log Out" button (right-aligned).

## Dependencies
- 01-001 (Okta SAML 2.0 Integration)

## Acceptance Criteria
- [ ] Unauthenticated state: CFF logo (left), "Log In" button (right)
- [ ] Authenticated state: CFF logo (clickable → cff.org), program name+ID, user name, "Log Out"
- [ ] Program selector dropdown visible for multi-program users
- [ ] User name displayed as "FirstName LastName"
- [ ] Header is persistent across all pages
- [ ] "Log Out" terminates session and redirects to home page
- [ ] Header is responsive: adjusts layout for tablet breakpoints

## Technical Notes
- Replace current AppBar/Drawer layout with the horizontal header specified in SRS
- Program selector dropdown triggers program context switch (see 03-004)
