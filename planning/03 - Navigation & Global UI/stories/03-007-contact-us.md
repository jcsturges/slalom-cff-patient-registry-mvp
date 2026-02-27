# Contact Us

**Story ID:** 03-007
**Epic:** 03 - Navigation & Global UI
**Priority:** P1
**SRS Reference:** Section 3.7.1.2

## User Story
As a user, I want to submit a help request through a Contact Us form so that I can get support from the Registry team.

## Description
Contact Us form accessible from the Help interface and as a link in the header/main menu. Form fields: Name (auto-populated), Email (auto-populated), Program Number (auto-populated), Subject (required), Message (required), Optional file attachment (≤5MB). The system stores the request and attachment in an encrypted store, sends a notification to reghelp@cff.org with a link to the secured location, generates a reference ID, shows a success message, and sends an auto-confirmation email to the user.

## Dependencies
- 01-001 (Okta SAML 2.0 Integration)

## Acceptance Criteria
- [ ] Form auto-populates Name, Email, Program Number from the user's session
- [ ] Subject and Message are required fields
- [ ] File attachment supports files up to 5MB
- [ ] "Send" button enabled only when required fields are filled
- [ ] Submission stores message and attachment in encrypted store
- [ ] Notification sent to reghelp@cff.org with link to secured location
- [ ] System generates a reference ID for tracking
- [ ] Success message displayed with reference ID
- [ ] Auto-confirmation email sent to user with their message and reference ID
- [ ] Clear error messages for missing fields or file upload issues
- [ ] Email sent within 10 seconds of submission

## Technical Notes
- Use Azure Blob Storage with encryption for attachments
- Consider a support ticket table to track submissions
- reghelp@cff.org notification should include a deep link to view the request (admin interface)
