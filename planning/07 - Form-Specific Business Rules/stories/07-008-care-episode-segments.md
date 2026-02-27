# Care Episode Segments

**Story ID:** 07-008
**Epic:** 07 - Form-Specific Business Rules
**Priority:** P0
**SRS Reference:** Section 6.6.6

## User Story
As a care program user, I want to enter Care Episodes with multiple segments representing different phases of care so that complex hospitalization histories are accurately captured.

## Description
Care Episode consists of one or more Segments, each with StartDateTime and optional EndDateTime. Overall Episode dates are calculated from segments (earliest start, latest end). Segment overlap validation: no overlapping segments within the same Episode or between different Episodes for the same patient. Only ONE open-ended segment (blank EndDateTime) per Episode. Overlap is defined as: EndDateTime of Segment1 ≥ StartDateTime of Segment2 AND EndDateTime of Segment2 ≥ StartDateTime of Segment1.

## Dependencies
- 06-001, 06-010

## Acceptance Criteria
- [ ] Care Episode consists of ≥1 Segments
- [ ] Each Segment has StartDateTime and optional EndDateTime
- [ ] Overall Episode StartDateTime = earliest segment StartDateTime
- [ ] Overall Episode EndDateTime = latest segment EndDateTime (blank if any segment open-ended)
- [ ] No overlapping segments within same Episode
- [ ] No overlapping Episodes for same patient
- [ ] Only one open-ended segment per Episode
- [ ] Overlap validation: EndDT1 ≥ StartDT2 AND EndDT2 ≥ StartDT1
- [ ] Open-ended segment treated as [Start, +∞) for overlap checks
- [ ] Validation errors identify which segments/episodes conflict

## Technical Notes
- This is one of the most complex validation rules in the system. Overlap checks must run on save.
