# Historical Data Migration

**Story ID:** 13-006
**Epic:** 13 - Data Feed & Migration
**Priority:** P0
**SRS Reference:** Sections 11

## User Story
As a system architect, I want to migrate historical data from portCF's Data Warehouse to the new Registry so that all existing patient data is available in the new system from day one.

## Description
All existing patient data is available in the new system from day one

## Dependencies
- 06-001, 04-001, 02-001

## Acceptance Criteria
- [ ] Shared form data migrated for all current and past patients (Demographics, Diagnosis, Sweat Test, ALD, Transplant)
- [ ] Program-specific data migrated for past 10 years (Annual Reviews, Encounters, Labs, Care Episodes, Phone Notes)
- [ ] Exclude file uploads for patients with date of death reported
- [ ] Re-runnable process supporting full and partial refresh
- [ ] Dedicated migration user account for attribution
- [ ] Each run logged: timestamp, record counts by entity, source system ID
- [ ] Migrated data appears in UI as if natively entered
- [ ] Field-specific migration mapping rules applied

## Technical Notes
- This is one of the highest-risk activities — requires extensive testing with production-like data.
