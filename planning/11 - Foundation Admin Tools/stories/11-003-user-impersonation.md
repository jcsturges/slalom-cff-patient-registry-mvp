# User Impersonation Activation

**Story ID:** 11-003
**Epic:** 11 - Foundation Admin Tools
**Priority:** P0
**SRS Reference:** Section 5.2.3

## User Story

As a Foundation Admin, I want to impersonate any CF care program user from the User Search Interface or Care Program Management screen so that I can investigate issues, verify user experiences, and provide support without needing the user's credentials.

## Description

User Impersonation allows Foundation Admins to temporarily assume the identity and permissions of any registry user. This story covers the activation flow: how an admin selects a target user and initiates the impersonation session. The admin's own FoundationAdmin privileges are suspended for the duration of the impersonation, and they operate with exactly the permissions of the impersonated user.

### Entry Points

1. **User Search Interface (Epic 04):** An "Impersonate" action button appears in the user search results and user detail views. Only visible to FoundationAdmin users.
2. **Care Program Management (Epic 02):** In the program user list, each user row has an "Impersonate" action. Only visible to FoundationAdmin users.

### Activation Flow

1. Admin clicks "Impersonate" on a target user.
2. A confirmation dialog appears: "You are about to impersonate [User Name] ([Role] at [Program]). You will lose your Foundation Admin privileges for the duration. Continue?"
3. On confirmation, the frontend calls `POST /api/admin/impersonation/start` with the target user's ID.
4. The API validates the request (caller must be FoundationAdmin, target must be an active user), creates an impersonation session record, and returns an impersonation token.
5. The frontend stores the impersonation token and switches context: all subsequent API calls include the impersonation token header.
6. The API's authorization middleware detects the impersonation token and resolves permissions from the target user's roles instead of the admin's roles.
7. The admin is redirected to the target user's default landing page.

### Authorization During Impersonation

- The impersonated session uses the **target user's** roles, claims, and program assignments.
- The admin's FoundationAdmin role is **not** available during impersonation.
- The only FoundationAdmin-level action available during impersonation is "Exit Impersonation" (covered in 11-004).
- If the target user has access to fewer programs or patients than the admin, the admin sees only what the target user would see.

### Session Constraints

- Impersonation sessions have a configurable maximum duration (default: 60 minutes).
- Only one active impersonation session per admin at a time.
- The session is tied to the admin's browser session; closing the browser ends the impersonation.
- Impersonation cannot be nested (an impersonating admin cannot impersonate a second user without first exiting).

## Dependencies

- **Epic 01:** Okta RBAC and JWT Bearer authentication must be in place.
- **Epic 04:** User Search Interface must exist for entry point 1.
- **Epic 02:** Care Program Management user list must exist for entry point 2.
- **Story 11-004:** Impersonation session UX (banner, exit button) — this story covers activation only.
- **Story 11-005:** Impersonation audit trail — all impersonation events must be logged.

## Acceptance Criteria

- [ ] An "Impersonate" button appears on user search results and user detail views for FoundationAdmin users only.
- [ ] An "Impersonate" action appears in the Care Program Management user list for FoundationAdmin users only.
- [ ] Clicking "Impersonate" shows a confirmation dialog with the target user's name, role, and program.
- [ ] The confirmation dialog warns that FoundationAdmin privileges will be suspended.
- [ ] On confirmation, the API creates an impersonation session and returns an impersonation token.
- [ ] The API rejects impersonation requests from non-FoundationAdmin users with 403 Forbidden.
- [ ] The API rejects impersonation requests targeting inactive or non-existent users with 400 Bad Request.
- [ ] After activation, all API calls use the target user's roles and claims for authorization.
- [ ] The admin cannot access FoundationAdmin-only features during impersonation (except Exit Impersonation).
- [ ] The admin sees only the programs, patients, and data that the target user has access to.
- [ ] Only one active impersonation session per admin is allowed; attempting a second returns 409 Conflict.
- [ ] Impersonation sessions expire after the configured maximum duration (default 60 minutes).
- [ ] Expired sessions automatically revert to the admin's normal context on the next API call.
- [ ] The impersonation start event is audit-logged with admin ID, target user ID, and timestamp.

## Technical Notes

### API Endpoints

- **POST** `/api/admin/impersonation/start`
  - Request body: `{ targetUserId: string }`
  - Response: `{ sessionId: string, token: string, targetUser: { id, name, role, program }, expiresAt: datetime }`
  - Authorization: `[Authorize(Policy = "FoundationAdmin")]`

- **POST** `/api/admin/impersonation/end`
  - Request body: `{ sessionId: string }`
  - Response: `{ success: true }`
  - Authorization: Requires valid impersonation session (special middleware check)

### Impersonation Token Design

The impersonation token is a signed JWT containing:
- `sub`: Target user's ID (used for authorization)
- `act.sub`: Acting admin's ID (used for audit)
- `imp_session_id`: Impersonation session GUID
- `exp`: Session expiration timestamp
- `iss`: Same issuer as normal tokens
- Standard claims from the target user's profile

The token is signed with the same key as regular JWTs but includes the `act` (actor) claim per RFC 8693 (Token Exchange).

### Frontend Integration

- **Impersonation context:** Create an `ImpersonationContext` (React Context) that stores the impersonation state.
- **API client interceptor:** When impersonation is active, the Axios/fetch interceptor adds the `X-Impersonation-Token` header to all requests.
- **Storage:** Store impersonation token in `sessionStorage` (not `localStorage`) so it is cleared when the browser tab closes.
- **Hook:** `useImpersonation()` — returns `{ isImpersonating, targetUser, startImpersonation, endImpersonation }`.

### Database

- **Table: `ImpersonationSessions`**
  - Columns: `Id` (GUID), `AdminUserId`, `TargetUserId`, `StartedAt`, `ExpiresAt`, `EndedAt` (nullable), `EndReason` (enum: Manual / Expired / BrowserClosed), `IsActive` (computed: EndedAt IS NULL AND ExpiresAt > GETUTCDATE()).
  - Index on `AdminUserId` WHERE `IsActive = true` for the one-session-per-admin constraint.

### Middleware

- **ImpersonationMiddleware:** Registered in the ASP.NET Core pipeline after authentication but before authorization. Inspects the `X-Impersonation-Token` header. If present and valid, replaces the `HttpContext.User` principal with one built from the target user's claims while preserving the acting admin's ID in `HttpContext.Items["ActingAdminId"]`.
