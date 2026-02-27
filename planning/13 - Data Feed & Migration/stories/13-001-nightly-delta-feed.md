# Nightly Delta Feed

**Story ID:** 13-001
**Epic:** 13 - Data Feed & Migration
**Priority:** P1
**SRS Reference:** Sections 10.1, 10.2

## User Story
As a system, I want to transmit all new and changed data nightly to the CFF Data Warehouse so that downstream processing has fresh data for reporting and analytics.

## Description
Downstream processing has fresh data for reporting and analytics

## Dependencies
- 06-001, 04-001

## Acceptance Criteria
- [ ] Data feed runs nightly
- [ ] Transmits: new records (creates), updated records, deleted/inactivated records
- [ ] Delta extraction based on last_updated_datetime and deleted_datetime/inactivated_datetime
- [ ] Supports controlled reprocessing for a specified date/time window
- [ ] Field naming consistent with CFF Data Model naming schema
- [ ] Data quality ≥99.9% accuracy

## Technical Notes
- Consider a change data capture (CDC) pattern or timestamp-based extraction.
