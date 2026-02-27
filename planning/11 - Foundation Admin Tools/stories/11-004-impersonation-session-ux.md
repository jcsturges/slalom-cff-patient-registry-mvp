# Impersonation Session UX

**Story ID:** 11-004
**Epic:** 11 - Foundation Admin Tools
**Priority:** P0
**SRS Reference:** Section 5.2.3

## User Story

As a Foundation Admin who is impersonating a user, I want a persistent visual indicator showing who I am impersonating and an easy way to exit impersonation so that I always know I am operating in an impersonated context and can return to my admin role at any time.

## Description

Once a Foundation Admin activates an impersonation session (story 11-003), the application must provide continuous visual feedback that the admin is operating as another user. This story covers the impersonation banner, the exit mechanism, and the navigation experience during impersonation.

### Impersonation Banner

A persistent, non-dismissible banner appears at the top of every page during an impersonation session. The banner:
- Displays: "You are impersonating [User Full Name] ([Role] at [Program Name])"
- Uses a distinctive color (e.g., deep orange or amber) that is visually distinct from standard alerts, warnings, or info banners.
- Includes a countdown timer showing remaining session time (e.g., "Session expires in 42:15").
- Contains an "Exit Impersonation" button prominently positioned on the right side.
- Is fixed to the top of the viewport (sticky positioning) so it is always visible, even when scrolling.
- Pushes the main navigation and content area down so no content is obscured.

### Exit Impersonation

Clicking "Exit Impersonation":
1. A brief confirmation appears: "Return to your Foundation Admin session?"
2. On confirm, the frontend calls `POST /api/admin/impersonation/end`.
3. The impersonation token is cleared from `sessionStorage`.
4. The `ImpersonationContext` is reset.
5. The admin is redirected to the Foundation Admin dashboard (or the page from which they initiated impersonation, if tracked).
6. The admin's full FoundationAdmin permissions are restored.

### Navigation During Impersonation

- The admin navigates the application exactly as the impersonated user would.
- The sidebar/navigation reflects the impersonated user's program assignments and role-based menu items.
- FoundationAdmin-only navigation items (e.g., Admin Tools, Database Lock) are hidden during impersonation since the admin is operating with the target user's permissions.
- The only exception is the impersonation banner and its "Exit Impersonation" button, which are always visible regardless of the impersonated user's role.

### Session Expiration

- When the impersonation session expires (timeout), the banner changes to: "Impersonation session has expired. Returning to admin context..."
- After a 3-second delay, the system automatically performs the exit impersonation flow.
- If the admin had unsaved work, it is preserved via the standard unsaved-changes guard (the page is not forcibly navigated until the user acknowledges).

### Edge Cases

- If the admin navigates directly to a URL that the impersonated user does not have access to, the standard role-gated access error (`<Alert severity="error">`) is shown.
- If the impersonated user's account is deactivated during the session, the impersonation session ends immediately with a notification.
- Browser back/forward navigation works normally; the impersonation context persists across client-side route changes.

## Dependencies

- **Story 11-003:** Impersonation activation must be implemented (this story handles the session UX after activation).
- **Epic 03:** Global navigation layout must support injecting the impersonation banner above the main content area.
- **Epic 01:** Role-based navigation filtering must be in place.

## Acceptance Criteria

- [ ] A persistent impersonation banner appears at the top of every page when impersonation is active.
- [ ] The banner displays: "You are impersonating [Full Name] ([Role] at [Program])".
- [ ] The banner uses a distinctive color (amber/orange) that is clearly different from standard alerts.
- [ ] The banner includes a countdown timer showing remaining session time in MM:SS format.
- [ ] The banner includes an "Exit Impersonation" button.
- [ ] The banner is sticky (fixed to the viewport top) and does not obscure any page content.
- [ ] Clicking "Exit Impersonation" shows a brief confirmation prompt.
- [ ] On confirm, the impersonation session ends, the token is cleared, and the admin is redirected to the Foundation Admin context.
- [ ] After exiting, the admin's full FoundationAdmin permissions are restored.
- [ ] During impersonation, the navigation sidebar reflects the impersonated user's role and program assignments.
- [ ] FoundationAdmin-only menu items are hidden during impersonation (except Exit Impersonation).
- [ ] When the session timer reaches zero, the banner changes to an expiration message and auto-exits after 3 seconds.
- [ ] If the admin has unsaved changes when the session expires, the unsaved-changes guard fires before navigation.
- [ ] Navigating to a URL the impersonated user lacks access to shows the standard role-gated error alert.
- [ ] The impersonation banner renders correctly on mobile viewports (responsive).
- [ ] The impersonation exit event is audit-logged (covered in 11-005 but must fire from this flow).

## Technical Notes

### Frontend Components

- **ImpersonationBanner.tsx** in `src/ngr-web-app/src/components/`:
  - Renders conditionally based on `useImpersonation().isImpersonating`.
  - Uses MUI `AppBar` or `Alert` component with `position: sticky`, `top: 0`, and a high `z-index` (above the main nav bar).
  - Color: MUI `warning` palette (amber) or a custom theme color.
  - Timer: Uses `useEffect` with `setInterval` to count down from `expiresAt - now`.

- **Layout integration:**
  - In the main `Layout.tsx` (or `App.tsx`), render `<ImpersonationBanner />` above the `<AppBar>` / `<Sidebar>` components.
  - When the banner is visible, add `padding-top` to the layout body equal to the banner height.

- **Navigation filtering:**
  - The sidebar navigation component already filters items by role (per RBAC conventions). During impersonation, `useRoles()` returns the impersonated user's roles (because the impersonation context overrides the auth context), so navigation filtering works automatically.

### State Management

- **ImpersonationContext** (from 11-003) provides:
  - `isImpersonating: boolean`
  - `targetUser: { id, name, role, program } | null`
  - `expiresAt: Date | null`
  - `initiatedFrom: string | null` (URL the admin was on when they started impersonation)
  - `endImpersonation: () => Promise<void>`

- **Session expiration watcher:** A `useEffect` in the `ImpersonationProvider` sets a timeout for the session expiration. When it fires, it calls `endImpersonation()` (with a 3-second grace period for the visual message).

### Accessibility

- The impersonation banner must have `role="status"` and `aria-live="polite"` so screen readers announce the impersonation state.
- The "Exit Impersonation" button must be keyboard-accessible and have a clear `aria-label`.
- The countdown timer should use `aria-hidden="true"` to avoid screen reader noise (the time remaining is informational, not critical).
