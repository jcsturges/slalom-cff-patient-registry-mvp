# Epic 07: Form-Specific Business Rules

## Overview

This epic covers the business rules unique to each of the 10 case report form types. While the generic form engine (Epic 06) handles common functionality like field rendering, validation, and status management, each form type has specific rules around gating (prerequisites), completion criteria, date handling, carry-forward behavior, sub-form structures, and inter-form relationships.

The two most critical gating rules are: (1) Demographics must be Complete before any other form can be created, and (2) Diagnosis must be Complete before most clinical forms can be entered. The Encounter and Labs & Tests forms have complex sub-form (tab) architectures with optional blocks. Care Episodes have a unique segment model with overlap validation. Annual Reviews enforce one-per-patient-per-program-per-year with carry-forward from prior years.

## Outcomes

- Demographics form acts as gating prerequisite for all other forms with auto-complete and redirect
- Diagnosis form acts as secondary gate for clinical forms with auto-complete
- Transplant forms correctly enforce organ-level uniqueness and step tracking
- Annual Review enforces year defaulting, one-per-year uniqueness, and carry-forward
- Encounter/Labs forms support sub-form selection, tab navigation, and carry-forward
- Care Episode segment model with overlap validation at both segment and episode level
- Phone Note date uniqueness enforced per program
- All form-specific completion criteria correctly implemented

## Key SRS References

- Section 6.6 — Form-specific requirements
- Section 6.6.1 — Demographics
- Section 6.6.2 — Diagnosis
- Section 6.6.3 — Transplant
- Section 6.6.4 — Annual Review Form
- Section 6.6.5 — Encounter and Labs and Tests Forms
- Section 6.6.6 — Care Episode Form

## Stories

| ID | Story | Priority | Dependencies |
|----|-------|----------|--------------|
| 07-001 | [Demographics Gating](stories/07-001-demographics-gating.md) | P0 | 06-001, 06-002 |
| 07-002 | [Diagnosis Gating](stories/07-002-diagnosis-gating.md) | P0 | 07-001, 06-008 |
| 07-003 | [Transplant Form Rules](stories/07-003-transplant-form-rules.md) | P0 | 06-001 |
| 07-004 | [Annual Review Rules](stories/07-004-annual-review-rules.md) | P0 | 06-001, 06-003 |
| 07-005 | [Encounter Sub-Forms](stories/07-005-encounter-sub-forms.md) | P0 | 06-007, 06-008 |
| 07-006 | [Encounter Carry-Forward](stories/07-006-encounter-carry-forward.md) | P1 | 07-005 |
| 07-007 | [Labs & Tests Rules](stories/07-007-labs-and-tests-rules.md) | P0 | 07-005 |
| 07-008 | [Care Episode Segments](stories/07-008-care-episode-segments.md) | P0 | 06-001, 06-010 |
| 07-009 | [Care Episode Completion](stories/07-009-care-episode-completion.md) | P0 | 07-008 |
| 07-010 | [Phone Note Rules](stories/07-010-phone-note-rules.md) | P0 | 06-001 |
