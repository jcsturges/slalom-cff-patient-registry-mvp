# Patient Record Change Tracking

**Story ID:** 12-002
**Epic:** 12 - Audit Logging & Analytics
**Priority:** P0
**SRS Reference:** Section 3.8.8.1

## User Story
As a CF Foundation Administrator, I want to view field-level change history for patient records so that I can audit who changed what and when for any patient.

## Description
I can audit who changed what and when for any patient

## Dependencies
- 12-001, 06-001

## Acceptance Criteria
- [ ] Every patient record change tracked: create, update, delete
- [ ] Captures: CFF ID, User ID + role, timestamp with timezone
- [ ] Captures: encounter/context identifier, object changed (form/section/field), field name
- [ ] Captures: previous value, new value (or 'value changed' with redaction)
- [ ] Captures: action type (Create, Update, Delete), change reason if collected
- [ ] Queryable by patient, user, date range
- [ ] Exportable via documented method

## Technical Notes
- Use EF Core interceptors or triggers to auto-capture changes. Consider temporal tables in SQL.
