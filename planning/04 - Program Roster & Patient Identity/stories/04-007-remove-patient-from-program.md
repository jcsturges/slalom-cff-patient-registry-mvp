# Remove Patient from Program

**Story ID:** 04-007
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P0
**SRS Reference:** Sections 4.3.2, 4.3.2.1

## User Story
As a care program administrator, I want to remove a patient from my program with a documented reason so that patients no longer receiving care are properly handled.

## Description
The Program Roster includes a "Remove from Program" button per patient. Clicking triggers a confirmation dialog requiring a Removal Reason: "Patient no longer seen within the program", "Patient withdrew consent", or "Consent issue/unable to verify consent". Upon confirmation: patient removed from current program roster, auto-associated with ORH if no other clinical programs remain, user returned to Program Roster (patient no longer listed).

**Special handling — Consent withdrawal:** If reason = "Patient withdrawing consent", the system removes patient from ALL clinical programs (not just current), records removal events for each, and sends notification to reghelp@cff.org.

## Dependencies
- 04-002 (Patient-Program Association)

## Acceptance Criteria
- [ ] "Remove from Program" button available for CP Admins and Editors
- [ ] Confirmation dialog requires selection of Removal Reason
- [ ] Three reasons: Patient no longer seen, Patient withdrew consent, Consent issue
- [ ] Cancel returns to roster with no changes
- [ ] Confirm removes patient from current program's roster
- [ ] If no other clinical programs remain, patient auto-associates with ORH
- [ ] Consent withdrawal: patient removed from ALL clinical programs
- [ ] Consent withdrawal: removal events recorded for each affected program
- [ ] Consent withdrawal: notification sent to reghelp@cff.org
- [ ] User returned to Program Roster after removal (patient no longer visible)

## Technical Notes
- Consent withdrawal is the most complex case — must iterate through all associations
- Consider a transaction to ensure atomicity of multi-program removal
- Consent-withdrawn patients must be excluded from patient search results
