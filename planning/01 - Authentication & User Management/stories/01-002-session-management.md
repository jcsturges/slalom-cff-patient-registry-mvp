# Session Management

**Story ID:** 01-002
**Epic:** 01 - Authentication & User Management
**Priority:** P0
**SRS Reference:** Section 2.3.1

## User Story
As a Registry user, I want my session to be managed securely with appropriate timeouts so that my data is protected when I'm inactive.

## Description
Implement session lifecycle management aligned with CFF session policies. Sessions are created only after successful SAML assertion validation. Session timeouts, renewal behavior, and logout handling must align with CFF's security requirements. The global header must display a "Log Out" button for authenticated users.

## Dependencies
- 01-001 (Okta SAML 2.0 Integration)

## Acceptance Criteria
- [ ] Application session is created only after successful SAML assertion validation
- [ ] Session timeout aligns with CFF session policies (configurable)
- [ ] Users are redirected to the home page after session expiration with a clear message
- [ ] "Log Out" button in the header terminates the session and redirects to the home page
- [ ] Concurrent sessions from the same user are handled appropriately
- [ ] Session tokens are secure (HttpOnly, Secure, SameSite attributes)
- [ ] No individual UI request exceeds 10 seconds without a progress indicator and cancel option

## Technical Notes
- Session timeout value should be configurable via environment variable or app settings
- Consider sliding expiration vs absolute expiration based on CFF policy
- Implement idle detection on the frontend to warn users before session expiry
