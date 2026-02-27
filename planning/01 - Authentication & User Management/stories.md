# Epic 01: Authentication & User Management

## Overview

This epic covers the complete authentication and authorization infrastructure for the NextGen Registry, including Okta SAML 2.0 integration, role-based access control (RBAC), and user management interfaces for both Care Program Administrators and CF Foundation Administrators.

The system must support approximately 1,000 users across approximately 280 care programs. Authentication is handled externally via CFF's Okta tenant (my.cff), while authorization (role assignments, program associations) is managed within the Registry application. Users are assigned one of four roles — three care-program-level roles and one foundation-level role — with the constraint that a user cannot hold both a Foundation Administrator role and a Care Program role simultaneously.

User management is distributed: Care Program Administrators manage users within their own program, while Foundation Administrators have global user management capabilities across all programs.

## Outcomes

- Users authenticate via CFF Okta SSO with SAML 2.0 and MFA enforcement
- Session lifecycle aligns with CFF session policies
- User metadata (name, email, user ID) is automatically synced from my.cff API on each login
- Four roles enforced across API and UI: CF Care Program Administrator, CF Care Program Editor, CF Care Program Report User, CF Foundation Administrator
- Care Program Administrators can manage users within their own program (add, change role, deactivate)
- Foundation Administrators can manage all users across all programs
- Deactivated users are preserved in historical records and labeled appropriately in the UI
- Email notifications sent for role assignments and program additions

## Key SRS References

- Section 2.1 — CF Care Program User Roles
- Section 2.2 — CF Foundation Administrator
- Section 2.3 — User Management (Okta Integration, User Metadata, CP Admin Management, Foundation Admin Management, Deactivated Users)
- Section 3.1 — Global Header (authentication state display)
- User Roles Table 1 (permissions matrix, referenced in Section 2.1)

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 01-001 | [Okta SAML 2.0 Integration](stories/01-001-okta-saml-integration.md) | P0 | None |
| 01-002 | [Session Management](stories/01-002-session-management.md) | P0 | 01-001 |
| 01-003 | [User Metadata Sync](stories/01-003-user-metadata-sync.md) | P0 | 01-001 |
| 01-004 | [Role Authorization Model](stories/01-004-role-authorization-model.md) | P0 | 01-001, 02-001 |
| 01-005 | [CP Admin User Management](stories/01-005-cp-admin-user-management.md) | P0 | 01-004 |
| 01-006 | [Foundation Admin User Management](stories/01-006-foundation-admin-user-management.md) | P0 | 01-004 |
| 01-007 | [User Deactivation Lifecycle](stories/01-007-user-deactivation-lifecycle.md) | P1 | 01-005, 01-006 |
| 01-008 | [Email Notifications](stories/01-008-email-notifications.md) | P1 | 01-005 |
