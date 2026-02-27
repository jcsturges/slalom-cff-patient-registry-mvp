# Email Notifications

**Story ID:** 01-008
**Epic:** 01 - Authentication & User Management
**Priority:** P1
**SRS Reference:** Section 2.3.3

## User Story
As a newly assigned user, I want to receive an email notification when I'm added to a care program so that I know I have access and what role I've been assigned.

## Description
When a user is assigned to a care program with a selected role, the system sends an email notification to the user: "You have been added to [Program Name] as [Role]". This applies whether the assignment was made by a CP Admin or a Foundation Admin.

## Dependencies
- 01-005 (CP Admin User Management)

## Acceptance Criteria
- [ ] Email sent when a user is assigned to a program: "You have been added to [Program Name] as [Role]"
- [ ] Email is sent regardless of whether the assignment was made by a CP Admin or Foundation Admin
- [ ] Email is sent to the user's registered email address
- [ ] Email is sent within 10 seconds of the assignment action
- [ ] System handles email delivery failures gracefully (logs error, does not block the assignment)
- [ ] Email content uses CFF branding/templates where applicable

## Technical Notes
- Use an email service (Azure Communication Services, SendGrid, etc.) for reliable delivery
- Consider a notification queue to handle transient email service failures
- Template should be configurable for future notification types
