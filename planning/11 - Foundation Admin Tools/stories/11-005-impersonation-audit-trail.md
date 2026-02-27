# Impersonation Audit Trail

**Story ID:** 11-005
**Epic:** 11 - Foundation Admin Tools
**Priority:** P1
**SRS Reference:** Section 5.2.3.2

## User Story
As a CF Foundation Administrator, I want to have all impersonation actions fully tracked in audit logs so that there is complete accountability for actions taken during impersonation sessions.

## Description
There is complete accountability for actions taken during impersonation sessions

## Dependencies
- 11-003, 12-001

## Acceptance Criteria
- [ ] All actions during impersonation attributed to both acting admin and impersonated user
- [ ] Audit logs include IsImpersonated flag
- [ ] Impersonation start/end timestamps recorded
- [ ] Impersonated user ID recorded
- [ ] Acting Foundation Admin user name recorded
- [ ] Impersonation sessions clearly distinguishable in audit trail

## Technical Notes
- Dual attribution (actor + effective user) is critical for compliance.
