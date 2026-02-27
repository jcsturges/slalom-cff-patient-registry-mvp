# Saved Download Definitions

**Story ID:** 09-003
**Epic:** 09 - Data Export & Raw Downloads
**Priority:** P2
**SRS Reference:** Section 8.2.1

## User Story
As a care program user, I want to save and reuse my download parameter sets so that I can quickly re-run common exports without reconfiguring each time.

## Description
I can quickly re-run common exports without reconfiguring each time

## Dependencies
- 09-001

## Acceptance Criteria
- [ ] Save download parameters as named definition with description
- [ ] Metadata stored: user name, description, date created, date modified, date last executed
- [ ] 'My Downloads' library with search/sort/filter
- [ ] Re-run from library without re-entering parameters
- [ ] Adjust time-period filters at runtime when re-running
- [ ] Edit/Save existing definition or Save As new
- [ ] Delete own definitions

## Technical Notes
- Saved definitions stored as JSON with user association.
