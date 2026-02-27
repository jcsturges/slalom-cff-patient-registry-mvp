# Care Program Admin User Management

**Story ID:** 01-005
**Epic:** 01 - Authentication & User Management
**Priority:** P0
**SRS Reference:** Section 2.3.3

## User Story
As a CF Care Program Administrator, I want to manage users within my program (view, add, change role, deactivate) so that I can control who has access to our program's patient data.

## Description
Build the User Management Interface accessible to CF Care Program Administrators. The interface shows users assigned to the admin's currently selected program with their roles. Admins can add new users (by email + role), change existing user roles, and deactivate users. CP Admins cannot operate on CF Foundation Administrator accounts.

Adding a user: enter email address, select one of the three CP roles. If the email exists in the system, assign the role. If not, call the my.cff API to register a new account and then assign the role. After assignment, send an email notification.

## Dependencies
- 01-004 (Role Authorization Model)

## Acceptance Criteria
- [ ] CP Admins see a user list for their currently selected program showing name, email, role
- [ ] CP Admins can add users by email address and role selection
- [ ] If user already exists in system, role is assigned to the program
- [ ] If user does not exist, system calls my.cff API to register, then assigns role
- [ ] CP Admins can change the role of existing users in their program
- [ ] CP Admins can deactivate users in their program
- [ ] CP Admins cannot see or operate on CF Foundation Administrator accounts
- [ ] After assigning a user, email notification sent: "You have been added to [Program Name] as [Role]"
- [ ] All user management actions are logged with timestamp and acting admin

## Technical Notes
- Interface is scoped to the admin's currently selected program context
- ~1000 users across ~280 programs means roughly 3-4 users per program on average
- my.cff API integration for user registration needs error handling for API failures
- Deactivation is per-program, not system-wide (see 01-007 for lifecycle details)
