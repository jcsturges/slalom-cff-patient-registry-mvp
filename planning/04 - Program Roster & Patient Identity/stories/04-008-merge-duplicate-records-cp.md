# Merge Duplicate Records — CP Admin

**Story ID:** 04-008
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P1
**SRS Reference:** Section 4.3.3.1

## User Story
As a care program administrator, I want to merge two duplicate patient records within my program so that the patient has a single consolidated record.

## Description
CP Admins access "Merge Duplicate Records" from the main menu. They search for patients by CFF ID, first name, or last name within their program (including ORH patients added to their roster). Search returns only patients in their program or ORH (no consent-withdrawn, no deceased patients). The admin identifies primary and secondary records, reviews demographics side-by-side, and confirms the merge. Patients associated with multiple CF centers can only be merged by Foundation Admins.

## Dependencies
- 04-003 (Program Roster View)

## Acceptance Criteria
- [ ] "Merge Duplicate Records" accessible from main menu for CP Admins
- [ ] Search by CFF ID, first name, or last name within program + ORH
- [ ] Excludes consent-withdrawn and deceased patients from search
- [ ] ORH patients must be added to roster before merging
- [ ] User selects primary and secondary records
- [ ] Side-by-side demographics comparison view
- [ ] Confirmation prompt: "I have reviewed both records" with proceed/cancel options
- [ ] Multi-program patients blocked from CP Admin merge (Foundation Admin required)

## Technical Notes
- Merge data consolidation rules defined in 04-010
- Consider a merge preview/dry-run before execution
- Merge should be a single transaction with rollback on failure
