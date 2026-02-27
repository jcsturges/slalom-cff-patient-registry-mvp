# Foundation Admin User Management

**Story ID:** 01-006
**Epic:** 01 - Authentication & User Management
**Priority:** P0
**SRS Reference:** Section 2.3.4

## User Story
As a CF Foundation Administrator, I want to manage all users across all programs from a centralized interface so that I can oversee access control for the entire Registry.

## Description
Build the global User Management Interface for Foundation Administrators. This provides a searchable, paginated list of all users across all programs. Foundation Admins can search by First Name, Last Name, email address, Okta ID, or organization. They can add new users to any organization (care program or CF Foundation) with any role, change role assignments, and deactivate users.

## Dependencies
- 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] Foundation Admins see all users across all programs in a searchable, paginated list
- [ ] Search supports: First Name, Last Name, email address, Okta ID, organization
- [ ] Pagination is available for the user list
- [ ] Foundation Admins can add new users to any care program or the CF Foundation with a selected role
- [ ] If user does not exist, system calls my.cff API to register and assign role
- [ ] Foundation Admins can change or deactivate role assignments for any user
- [ ] Foundation Admins can assign the CF Foundation Administrator role
- [ ] When deactivating from all programs, system calls my.cff API to remove from access group
- [ ] All user management actions are logged with timestamp and acting admin

## Technical Notes
- Users associated with multiple programs appear once per association (one row per user-program pair)
- The interface must clearly show which programs a user is associated with and their role in each
- Consider a user detail view that shows all program associations for a selected user
