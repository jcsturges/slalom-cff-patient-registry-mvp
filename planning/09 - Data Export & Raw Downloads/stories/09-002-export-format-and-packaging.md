# Export Format & Packaging

**Story ID:** 09-002
**Epic:** 09 - Data Export & Raw Downloads
**Priority:** P1
**SRS Reference:** Section 8.2

## User Story
As a care program user, I want to choose between coded and descriptive output formats and receive a structured download so that downloaded data is ready for analysis in my preferred format.

## Description
Downloaded data is ready for analysis in my preferred format

## Dependencies
- 09-001

## Acceptance Criteria
- [ ] Two content formats: 'As codes' (numeric/coded values) and 'As descriptives' (human-readable labels)
- [ ] Format applied consistently to all coded fields in download
- [ ] Output as structured CSV files
- [ ] Multiple forms packaged as ZIP archive
- [ ] One file per form type with clear naming convention
- [ ] Indicate which completeness option was selected when download was executed

## Technical Notes
- Consider background job for large exports with download link notification.
