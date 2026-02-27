# Okta SAML 2.0 Integration

**Story ID:** 01-001
**Epic:** 01 - Authentication & User Management
**Priority:** P0
**SRS Reference:** Sections 2.3.1, 3.1, 3.2

## User Story
As a Registry user, I want to authenticate via my CFF Okta (my.cff) account using SAML 2.0 so that I can access the Registry with Single Sign-On and multi-factor authentication.

## Description
Integrate the Registry with CFF's Okta tenant using the SAML 2.0 protocol. The unauthenticated home page displays a "Log In" button that redirects to Okta for authentication. Upon successful SAML assertion validation, the system creates an application session. The system must rely on Okta to enforce Multi-Factor Authentication (MFA) policies — the Registry itself does not implement MFA but must only create sessions after successful SAML validation.

## Dependencies
- None (foundational story)

## Acceptance Criteria
- [ ] Clicking "Log In" on the home page redirects to CFF Okta SAML authentication
- [ ] Successful SAML assertion creates an authenticated application session
- [ ] The system validates the SAML assertion (signature, issuer, audience, timestamps)
- [ ] Failed SAML validation displays a clear error message and does not create a session
- [ ] MFA is enforced via Okta policies (the system does not bypass or duplicate MFA)
- [ ] SSO works — users already authenticated in Okta bypass the login prompt
- [ ] The system correctly handles SAML logout (single logout if supported)
- [ ] Configuration supports environment-specific Okta tenant settings (issuer, callback URLs)

## Technical Notes
- Current MVP uses OIDC/PKCE via okta-react — SRS specifies SAML 2.0 for production. Evaluate whether to keep OIDC for SPA and use SAML only for API/backend, or switch entirely.
- SAML assertion should provide user email as the unique identifier for user lookup
- Consider using passport-saml or similar library for .NET SAML validation
- Session token should be issued by the API after SAML validation, not stored as raw SAML assertion
