# Duplicate Detection

**Story ID:** 04-005
**Epic:** 04 - Program Roster & Patient Identity
**Priority:** P0
**SRS Reference:** Sections 4.3.1.2, 4.3.1.3, 4.3.1.4

## User Story
As a system, I want to perform background duplicate detection when adding a patient so that duplicate records are identified before creation.

## Description
Upon clicking Continue on the Patient Identity Form, the system performs a background match search against all existing patient records (including ORH, excluding consent-withdrawn). The search is non-blocking by default.

**Matching logic (two approaches):**
A) Registry ID provided: require one additional field match (first name, last name, or DOB) for exact match.
B) Registry ID not provided: fuzzy matching using at minimum last name, DOB, biological sex at birth, plus first name similarity. Eight sample fuzzy rules ranging from high similarity (exact name + close DOB) to phonetic similarity with flexible DOB.

**Match presentation:** If matches found, display non-blocking banner with match list (Registry ID, name, last name at birth, DOB, program associations, ORH indicator). User can select an existing patient or proceed with creating new. High-confidence matches require explicit confirmation.

If >3 matches, display message requesting additional info or contacting Registry Support.

## Dependencies
- 04-004 (Patient Identity Form)

## Acceptance Criteria
- [ ] Background match search runs automatically upon clicking Continue
- [ ] Search includes all patients in Registry including ORH (excludes consent-withdrawn)
- [ ] Registry ID match requires one additional field match
- [ ] Fuzzy matching implements at least the 8 specified rules
- [ ] Match list displays: Registry ID, name, last name at birth, DOB, program associations, ORH indicator
- [ ] User can select existing patient (re-acquisition) or proceed with new record
- [ ] High-confidence matches require explicit user confirmation or override with warning
- [ ] >3 matches triggers message requesting additional info
- [ ] If no matches found, proceed directly to Demographics form with new CFF ID

## Technical Notes
- Matching engine should be implemented server-side for security (prevents registry ID enumeration)
- Consider using Levenshtein distance or Soundex/Metaphone for phonetic matching
- Configurable match threshold for tuning sensitivity
- This is a complex subsystem — consider building as a standalone service
