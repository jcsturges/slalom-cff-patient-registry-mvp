# User Metadata Sync

**Story ID:** 01-003
**Epic:** 01 - Authentication & User Management
**Priority:** P0
**SRS Reference:** Section 2.3.2

## User Story
As a system administrator, I want user metadata to be automatically synchronized from the my.cff user management API on each login so that Registry user records stay current without manual updates.

## Description
After each login, the system asynchronously queries the my.cff user management API by email address to retrieve and store user metadata: first name, last name, email, and user ID. The user ID (from my.cff) serves as the permanent unique identifier internally. The display title is formatted as "First Initial + Last Name" (e.g., "J Smith"). Any changes to user metadata are logged.

## Dependencies
- 01-001 (Okta SAML 2.0 Integration)

## Acceptance Criteria
- [ ] System queries my.cff API asynchronously after every login
- [ ] User metadata stored: first name, last name, email, user ID
- [ ] User ID from my.cff is used as the permanent unique identifier
- [ ] Display title renders as First Initial + Last Name (e.g., "J Smith") in the header
- [ ] Name and email changes from my.cff are reflected in the Registry after next login
- [ ] All updates to user information are logged with old and new values
- [ ] If the my.cff API is unavailable, the login still succeeds using cached metadata
- [ ] Name/email changes are governed by my.cff — the Registry does not allow direct edits to these fields

## Technical Notes
- Implement as a background job triggered post-authentication, not blocking login
- Cache user metadata locally to handle my.cff API downtime
- Consider a periodic batch sync in addition to per-login sync for users who haven't logged in recently
