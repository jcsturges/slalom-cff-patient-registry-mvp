# Merge Data Consolidation

**Story ID:** 04-010
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P1
**SRS Reference:** Sections 4.3.3.3, 4.3.3.4, 4.3.3.5

## User Story
As a system, I want to correctly consolidate data when two patient records are merged so that all clinical data, forms, and files are properly handled.

## Description
When a merge is executed:

**Metadata consolidation:** Secondary record's identifiers (CFF ID, names, last name at birth) added as aliases to primary record. Primary record's program associations become the union of both records' associations. Secondary record removed from all program rosters.

**Form retention:** All primary record forms retained. Secondary shared forms (Demographics, Diagnosis, Sweat Test, ALD, Transplant) deleted. Secondary program-specific forms (Annual Review, Encounter, Labs, Care Episode, Phone Note) reassigned to primary. Date-based conflict resolution: if both have the same form for same program on same date, secondary's form is deleted.

**File consolidation:** All files from both records retained and reassigned to primary record.

## Dependencies
- 04-008 (Merge Records — CP Admin)

## Acceptance Criteria
- [ ] Secondary CFF ID and names added as aliases to primary record
- [ ] Program associations merged (union of both records)
- [ ] Secondary record removed from all rosters and not visible
- [ ] All primary forms retained
- [ ] Secondary shared forms deleted
- [ ] Secondary program-specific forms reassigned to primary
- [ ] Date-based conflicts: secondary's form deleted (Annual Review by year, Encounter by date, etc.)
- [ ] All files from both records retained and reassigned to primary
- [ ] Merge event logged in audit trail with primary/secondary IDs, timestamp, user

## Technical Notes
- Merge must be atomic (transaction) — partial merges must roll back
- Consider a pre-merge validation step that identifies conflicts
- Date conflict rules: Annual Review (same year+program), Encounter (same date+program), Labs (same date+program), Care Episode (same start date+program), Phone Note (same date+program)
