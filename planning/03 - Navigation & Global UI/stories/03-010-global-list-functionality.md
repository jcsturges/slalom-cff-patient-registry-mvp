# Global List Functionality

**Story ID:** 03-010
**Epic:** 03 - Navigation & Global UI
**Priority:** P0
**SRS Reference:** Section 3.9

## User Story
As a user viewing any list in the Registry, I want consistent pagination, search, and sort functionality so that I can efficiently navigate large datasets.

## Description
All list views in the Registry must support three capabilities: Pagination (default 25 rows, toggle 25/50/100, or load-as-you-scroll), Search (partial match on any displayed column, persists across pages), and Sort (any column, ascending/descending, respects data type, persists across pages). Build these as reusable components.

## Dependencies
- None (reusable component)

## Acceptance Criteria
- [ ] Default load is 25 rows
- [ ] User can toggle between 25, 50, or 100 rows per page (if pagination-based)
- [ ] Search supports partial match on any field displayed as a column
- [ ] Search criteria persist when navigating between pages
- [ ] All columns are sortable (ascending/descending toggle)
- [ ] Sort respects the underlying data type (number, date, text)
- [ ] Sort order persists when navigating between pages
- [ ] Performance is maintained even with underlying datasets of 1M+ rows
- [ ] Page remains usable regardless of query performance

## Technical Notes
- Build as reusable React components (DataTable, SearchBar, Pagination)
- Server-side pagination/search/sort for large datasets
- API endpoints should accept `page`, `pageSize`, `search`, `sortBy`, `sortDirection` parameters
