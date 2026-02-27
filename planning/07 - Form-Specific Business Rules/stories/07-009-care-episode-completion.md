# Care Episode Completion

**Story ID:** 07-009
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.6.7

## User Story
As a care program user, I want Care Episode completion rules to require all segments to be closed so that only finalized episodes are marked Complete.

## Description
A Care Episode cannot be marked Complete unless ALL segments have both StartDateTime and EndDateTime (no blank EndDateTimes). The form can be saved as Incomplete with open-ended segments.

## Dependencies
- 07-008

## Acceptance Criteria
- [ ] Cannot Mark Complete if any segment has blank EndDateTime
- [ ] Can Save as Incomplete with open-ended segments
- [ ] Complete status is user-specified (after validation)
- [ ] Validation message clearly identifies which segments need EndDateTime

## Technical Notes
- Simple rule but critical for data quality — ensures completeness of temporal data.
