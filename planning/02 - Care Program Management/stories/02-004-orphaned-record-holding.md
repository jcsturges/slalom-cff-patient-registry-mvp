# Orphaned Record Holding (ORH)

**Story ID:** 02-004
**Epic:** 02 - Care Program Management
**Priority:** P0
**SRS Reference:** Section 3.8.1.4, 4.2.1

## User Story
As a system, I want to automatically associate orphaned patients with a dedicated ORH program so that no patient record is ever lost when they are removed from all clinical care programs.

## Description
Implement the Orphaned Record Holding program (historically Program ID 3000). This is a system-reserved administrative program that safeguards patient records when they have no active clinical program associations.

**Rules:**
- A patient is "orphaned" when they have no Active patient-program associations to any clinical care program
- When orphaned, system auto-creates/maintains an ORH association with Active state
- ORH is system-reserved: not deletable, not editable in identity, not deactivatable

**Access Control:**
- CF care program users **cannot**: view ORH in program lists, be assigned to ORH, navigate to ORH roster, modify data "as ORH user"
- CF Foundation Administrators have full administrative access to ORH roster/tools

**Search and Re-acquisition:**
- Patients in ORH are discoverable via standard patient search
- CP users can add ORH patients to their roster (creating a new program association, which removes the orphan status)

## Dependencies
- 02-001 (Care Program Data Model)

## Acceptance Criteria
- [ ] ORH program exists as a system-seeded record (cannot be created or deleted by users)
- [ ] When a patient has no active clinical program associations, system auto-associates with ORH
- [ ] ORH does not appear in care program user interfaces (program selector, program list)
- [ ] Foundation Admins can access ORH roster via the Program List
- [ ] Patients in ORH are discoverable via Patient Search (excluding consent withdrawal)
- [ ] CP users can re-acquire ORH patients to their roster (creates new association)
- [ ] Re-acquisition removes orphan status if patient now has at least one clinical program association
- [ ] ORH cannot be deactivated, deleted, or have its identity edited

## Technical Notes
- Seed ORH program via EF Core data seeding or migration
- ORH association should be transparent: auto-managed by the system, not manually created
- Consider a database trigger or application-level event handler for orphan detection
