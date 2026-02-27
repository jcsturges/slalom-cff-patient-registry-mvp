# EMR Field Mapping

**Story ID:** 10-003
**Epic:** 10 - EMR Upload Integration
**Priority:** P1
**SRS Reference:** Section 9

## User Story
As a system architect, I want to map CSV fields to Registry form fields for Demographics, Encounter, and Labs so that EMR data correctly populates the right form fields.

## Description
EMR data correctly populates the right form fields

## Dependencies
- 06-001, 06-007

## Acceptance Criteria
- [ ] Mapping covers ~240 fields from Demographics, Encounter, Labs and Tests
- [ ] System allows adding new CSV fields without breaking existing mappings
- [ ] MRN-to-CFF ID crosswalk maintained per institution
- [ ] Only patients with established Registry records receive EMR data
- [ ] Field mapping is configurable per institution

## Technical Notes
- Field mapping configuration should be maintainable by Foundation Admins or via configuration files.
