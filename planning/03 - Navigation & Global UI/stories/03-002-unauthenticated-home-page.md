# Unauthenticated Home Page

**Story ID:** 03-002
**Epic:** 03 - Navigation & Global UI
**Priority:** P0
**SRS Reference:** Section 3.2

## User Story
As an unauthenticated visitor, I want to see a branded landing page with announcements and a login button so that I understand what the system is and how to access it.

## Description
The home page for unauthenticated users displays: CFF logo, brief description of the NextGen Registry (2-3 sentences), active announcements, "Log In" button linking to CFF Okta authentication (SAML), and footer with "Privacy Policy" and "Terms & Conditions" links.

## Dependencies
- 03-001 (Global Header)

## Acceptance Criteria
- [ ] CFF logo displayed prominently
- [ ] Brief NGR description (2-3 sentences) is visible
- [ ] Active announcements are displayed
- [ ] "Log In" button initiates Okta SAML authentication
- [ ] Footer contains "Privacy Policy" and "Terms & Conditions" links
- [ ] Page is visually branded with CFF design language

## Technical Notes
- Announcements should be fetched from the same announcement system used by authenticated users (03-008)
- Footer links may be external URLs configured via app settings
