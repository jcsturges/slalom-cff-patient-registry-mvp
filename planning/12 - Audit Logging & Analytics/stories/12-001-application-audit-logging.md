# Application Audit Logging

**Story ID:** 12-001
**Epic:** 12 - Audit Logging & Analytics
**Priority:** P0
**SRS Reference:** Section 1.8.1

## User Story
As a system architect, I want to implement comprehensive action logging across the system so that all significant events are captured for security, compliance, and troubleshooting.

## Description
All significant events are captured for security, compliance, and troubleshooting

## Dependencies
- None

## Acceptance Criteria
- [ ] All major actions logged with date/timestamp, user ID, program ID
- [ ] Actions logged: view, create, modify, delete, archive, PHI access, file operations, merge, status changes
- [ ] User IDs and event dates/times captured
- [ ] Successful and rejected access attempts logged
- [ ] Privilege evaluation attempts logged
- [ ] Account management activities logged (role changes, deactivation)
- [ ] Source and destination IP addresses captured
- [ ] CRITICAL: All logs EXCLUDE PHI, PII, and patient content
- [ ] Logs contain only opaque identifiers (CFF ID, user ID, resource IDs)

## Technical Notes
- Use structured logging (Serilog with JSON output) for queryability. Consider a dedicated audit log table separate from application logs.
