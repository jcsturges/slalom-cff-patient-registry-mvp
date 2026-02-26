# Acceptance Criteria
## Next Generation Patient Registry (NGR) - Cystic Fibrosis Foundation

**Document Version:** 1.0  
**Date:** 2025  
**Project:** NGR - Next Generation Registry

---

## Overview

This document consolidates all acceptance criteria for the NGR project. Each criterion is:
- Specific and measurable
- Testable by automated systems or QA
- Linked to a user story ID

---

## 1. Authentication & Authorization

### AC-1.1: SSO Integration
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-1.1.1 | System redirects unauthenticated users to SSO provider | US-001 | Automated |
| AC-1.1.2 | Successful SSO authentication creates valid session | US-001 | Automated |
| AC-1.1.3 | Failed SSO authentication displays error message | US-001 | Automated |
| AC-1.1.4 | Expired SSO session triggers re-authentication | US-001 | Automated |
| AC-1.1.5 | Session timeout follows security policy (configurable) | US-001 | Automated |

### AC-1.2: Role-Based Access Control
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-1.2.1 | Users can only access functions permitted by their role | US-004 | Automated |
| AC-1.2.2 | Care Program Users see only their program's patients | US-004 | Automated |
| AC-1.2.3 | Foundation Admins can view patients across all programs | US-004 | Automated |
| AC-1.2.4 | Menu items are filtered based on user role | US-004 | Automated |
| AC-1.2.5 | Unauthorized access attempts are logged | US-004 | Automated |
| AC-1.2.6 | Role changes take effect immediately without re-login | US-004 | Automated |

### AC-1.3: User Management
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-1.3.1 | Care Program Admins can add users to their program | US-002 | Automated |
| AC-1.3.2 | Care Program Admins can deactivate users in their program | US-002 | Automated |
| AC-1.3.3 | Care Program Admins can modify user roles within their program | US-002 | Automated |
| AC-1.3.4 | Care Program Admins cannot see users from other programs | US-002 | Automated |
| AC-1.3.5 | Foundation Admins can search users across all programs | US-003 | Automated |
| AC-1.3.6 | Foundation Admins can filter users by program, role, status | US-003 | Automated |
| AC-1.3.7 | All user management actions are logged with timestamp | US-003 | Automated |

---

## 2. Patient Roster Management

### AC-2.1: Add Patient
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-2.1.1 | New patient enrollment form displays all required fields | US-005 | Automated |
| AC-2.1.2 | Valid patient data submission creates patient record | US-005 | Automated |
| AC-2.1.3 | Invalid/incomplete data displays validation errors | US-005 | Automated |
| AC-2.1.4 | New patient appears in program roster after enrollment | US-005 | Automated |
| AC-2.1.5 | Patient receives unique identifier (DWID) | US-005 | Automated |

### AC-2.2: Transfer Patient
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-2.2.1 | Transfer initiation allows selection of destination program | US-006 | Automated |
| AC-2.2.2 | Completed transfer moves patient to receiving program roster | US-006 | Automated |
| AC-2.2.3 | Transferred patient removed from originating program roster | US-006 | Automated |
| AC-2.2.4 | All historical data accessible to receiving program | US-006 | Automated |
| AC-2.2.5 | Transfer action logged with source, destination, timestamp | US-006 | Automated |

### AC-2.3: Merge Records
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-2.3.1 | Merge allows selection of primary record | US-007 | Automated |
| AC-2.3.2 | Merge preview shows data consolidation result | US-007 | Manual |
| AC-2.3.3 | Completed merge preserves all historical data | US-007 | Automated |
| AC-2.3.4 | Merged secondary record no longer appears in searches | US-007 | Automated |
| AC-2.3.5 | Merge action logged with both record IDs | US-007 | Automated |

### AC-2.4: Remove Patient
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-2.4.1 | Removal requires reason selection | US-008 | Automated |
| AC-2.4.2 | Removed patient not visible in active roster | US-008 | Automated |
| AC-2.4.3 | Removed patient historical data accessible via search filters | US-008 | Automated |
| AC-2.4.4 | Removal action logged with reason | US-008 | Automated |

### AC-2.5: Search Roster
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-2.5.1 | Search filters results in real-time | US-009 | Automated |
| AC-2.5.2 | Multiple filters apply with AND logic | US-009 | Automated |
| AC-2.5.3 | Patient selection navigates to patient summary | US-009 | Automated |
| AC-2.5.4 | No results displays appropriate message | US-009 | Automated |

---

## 3. Case Report Forms (eCRFs)

### AC-3.1: Form Rendering
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-3.1.1 | Forms render according to machine-readable specification | US-010 | Automated |
| AC-3.1.2 | All field types render correctly (text, dropdown, date, etc.) | US-010 | Automated |
| AC-3.1.3 | Field labels and help text display correctly | US-010 | Automated |
| AC-3.1.4 | Form sections and groupings render as specified | US-010 | Automated |
| AC-3.1.5 | Forms load within performance specifications | US-010 | Automated |

### AC-3.2: Conditional Logic
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-3.2.1 | Conditional fields show when condition is met | US-011 | Automated |
| AC-3.2.2 | Conditional fields hide when condition is not met | US-011 | Automated |
| AC-3.2.3 | Changing trigger value updates dependent fields | US-011 | Automated |
| AC-3.2.4 | Hidden field data handled per business rules | US-011 | Automated |

### AC-3.3: Validation
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-3.3.1 | Invalid data displays inline error message | US-012 | Automated |
| AC-3.3.2 | Required field validation prevents save with missing data | US-012 | Automated |
| AC-3.3.3 | Corrected data clears error message | US-012 | Automated |
| AC-3.3.4 | All validations passing allows successful save | US-012 | Automated |
| AC-3.3.5 | Field option consistency is 100% | US-012 | Automated |

### AC-3.4: Derived Fields
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-3.4.1 | Derived fields auto-calculate from source values | US-013 | Automated |
| AC-3.4.2 | Derived fields are read-only | US-013 | Automated |
| AC-3.4.3 | Derived fields recalculate when source values change | US-013 | Automated |
| AC-3.4.4 | Derived field calculations are 100% accurate | US-013 | Automated |

### AC-3.5: Ownership and Permissions
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-3.5.1 | Forms owned by user's program are editable | US-014 | Automated |
| AC-3.5.2 | Forms owned by other programs are view-only | US-014 | Automated |
| AC-3.5.3 | Sharing rules correctly applied to shared forms | US-014 | Automated |
| AC-3.5.4 | Expired edit windows result in view-only access | US-014 | Automated |

### AC-3.6: Auto-Save
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-3.6.1 | Form data auto-saves at defined interval | US-015 | Automated |
| AC-3.6.2 | Session interruption allows resume from auto-saved draft | US-015 | Manual |
| AC-3.6.3 | Auto-save displays subtle confirmation indicator | US-015 | Manual |
| AC-3.6.4 | Auto-save failure notifies user | US-015 | Automated |

---

## 4. Data Import

### AC-4.1: CSV Upload
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-4.1.1 | CSV upload utility accepts file selection | US-016 | Automated |
| AC-4.1.2 | Valid CSV pre-fills matching eCRF fields | US-016 | Automated |
| AC-4.1.3 | Pre-filled data available for review before final save | US-016 | Manual |
| AC-4.1.4 | Data transformations are 100% accurate | US-016 | Automated |

### AC-4.2: CSV Validation
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-4.2.1 | Invalid file format displays clear error message | US-017 | Automated |
| AC-4.2.2 | Data validation failures identify specific rows/fields | US-017 | Automated |
| AC-4.2.3 | Error details are downloadable | US-017 | Automated |
| AC-4.2.4 | Corrected re-upload processes previously failed records | US-017 | Automated |

---

## 5. Reporting

### AC-5.1: Dynamic Report Builder
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-5.1.1 | Report builder allows selection of filter criteria | US-018 | Automated |
| AC-5.1.2 | Multiple criteria apply with AND logic | US-018 | Automated |
| AC-5.1.3 | Report results display matching patient list | US-018 | Automated |
| AC-5.1.4 | Queries can be saved for future use | US-018 | Automated |

### AC-5.2: Standard Reports
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-5.2.1 | Standard reports list is accessible | US-019 | Automated |
| AC-5.2.2 | Care Program Users see program-specific results | US-019 | Automated |
| AC-5.2.3 | Foundation Users see cross-program results | US-019 | Automated |
| AC-5.2.4 | Reports exportable in standard formats | US-019 | Automated |

### AC-5.3: Export
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-5.3.1 | Export format selection is available | US-020 | Automated |
| AC-5.3.2 | CSV export produces properly formatted file | US-020 | Automated |
| AC-5.3.3 | PHI exports apply appropriate security measures | US-020 | Automated |
| AC-5.3.4 | Large exports display progress indicator | US-020 | Manual |

---

## 6. Content Management

### AC-6.1: Documentation
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-6.1.1 | Foundation Admins can create documentation | US-021 | Automated |
| AC-6.1.2 | Published documentation visible to care program users | US-021 | Automated |
| AC-6.1.3 | Documentation editable without developer assistance | US-021 | Manual |
| AC-6.1.4 | Updated documentation immediately available | US-021 | Automated |

### AC-6.2: Announcements
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-6.2.1 | Announcements support visibility and expiration settings | US-022 | Automated |
| AC-6.2.2 | Active announcements display prominently on login | US-022 | Automated |
| AC-6.2.3 | Expired announcements no longer display | US-022 | Automated |
| AC-6.2.4 | Announcements are editable after creation | US-022 | Automated |

### AC-6.3: User View
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-6.3.1 | Active announcements visible on dashboard | US-023 | Automated |
| AC-6.3.2 | Documentation accessible via help section | US-023 | Automated |
| AC-6.3.3 | Documents display current published version | US-023 | Automated |
| AC-6.3.4 | Dismissible announcements can be dismissed | US-023 | Automated |

---

## 7. Data Integration

### AC-7.1: Data Warehouse Feed
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-7.1.1 | Nightly feed transmits all new/modified data | US-024 | Automated |
| AC-7.1.2 | Nightly processing completes in <3 hours | US-024 | Automated |
| AC-7.1.3 | Data transformations are 100% accurate | US-024 | Automated |
| AC-7.1.4 | Feed failures generate alerts | US-024 | Automated |

### AC-7.2: Monitoring
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-7.2.1 | Data feed status visible to Foundation Admins | US-025 | Automated |
| AC-7.2.2 | Feed logs show record counts and processing time | US-025 | Automated |
| AC-7.2.3 | Failed feed logs show error details | US-025 | Automated |
| AC-7.2.4 | Historical feed results are accessible | US-025 | Automated |

---

## 8. Data Migration

### AC-8.1: Historical Migration
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-8.1.1 | All patient records migrated from PortCF | US-026 | Automated |
| AC-8.1.2 | DWID match is 100% | US-026 | Automated |
| AC-8.1.3 | Same-to-same variables match 100% | US-026 | Automated |
| AC-8.1.4 | Derived fields match 100% | US-026 | Automated |
| AC-8.1.5 | Field consistency is 100% | US-026 | Automated |

### AC-8.2: Validation
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-8.2.1 | Validation produces comparison reports | US-027 | Automated |
| AC-8.2.2 | Discrepancies are identifiable for investigation | US-027 | Manual |
| AC-8.2.3 | Resolved discrepancies clear on re-validation | US-027 | Automated |
| AC-8.2.4 | All criteria met certifies migration complete | US-027 | Automated |

### AC-8.3: Parallel Operation
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-8.3.1 | PortCF data entry for prior year functions correctly | US-028 | Manual |
| AC-8.3.2 | NGR data entry for current year functions correctly | US-028 | Manual |
| AC-8.3.3 | Catch-up migration transfers remaining PortCF data | US-028 | Automated |
| AC-8.3.4 | Users understand which system to use | US-028 | Manual |

---

## 9. System Administration

### AC-9.1: Environment Isolation
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-9.1.1 | Each environment is fully isolated | US-029 | Automated |
| AC-9.1.2 | Security controls prevent lateral movement | US-029 | Penetration Test |
| AC-9.1.3 | Network segmentation verified for shared subscriptions | US-029 | Automated |
| AC-9.1.4 | Training environment is copy of production | US-029 | Manual |

### AC-9.2: Audit Logging
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-9.2.1 | All user actions logged with timestamp and user ID | US-030 | Automated |
| AC-9.2.2 | Audit logs searchable by user, action, date | US-030 | Automated |
| AC-9.2.3 | PHI access is logged for compliance | US-030 | Automated |
| AC-9.2.4 | Audit log retention follows policy | US-030 | Automated |

---

## 10. Training Support

### AC-10.1: Training Environment
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-10.1.1 | Training environment mirrors production functionality | US-031 | Manual |
| AC-10.1.2 | Training changes do not affect production | US-031 | Automated |
| AC-10.1.3 | Training environment can be reset | US-031 | Manual |
| AC-10.1.4 | Features work as documented in training materials | US-031 | Manual |

### AC-10.2: Training Documentation
| ID | Criterion | Story | Testable By |
|----|-----------|-------|-------------|
| AC-10.2.1 | Training materials accessible within system | US-032 | Automated |
| AC-10.2.2 | Training materials are searchable | US-032 | Automated |
| AC-10.2.3 | Contextual help available on screens | US-032 | Automated |
| AC-10.2.4 | Training materials show current version | US-032 | Automated |

---

## 11. Performance Criteria

| ID | Criterion | Threshold | Testable By |
|----|-----------|-----------|-------------|
| AC-PERF-001 | Page/form load time | Per Appendix A specifications | Automated |
| AC-PERF-002 | Nightly data processing | <3 hours | Automated |
| AC-PERF-003 | Concurrent user support | 136 care centers (~3,000+ users) | Load Test |
| AC-PERF-004 | Data volume support | ~1 TB | Automated |
| AC-PERF-005 | Patient record capacity | 35,000 active + growth | Automated |
| AC-PERF-006 | Encounter capacity | 4 million + 120,000/year growth | Automated |

---

## 12. Security Criteria

| ID | Criterion | Threshold | Testable By |
|----|-----------|-----------|-------------|
| AC-SEC-001 | PHI confidentiality, integrity, availability | HIPAA compliant | Audit |
| AC-SEC-002 | Third-party security validation | SOC 2 Type 2, HITRUST, or ISO 27001 | Certification |
| AC-SEC-003 | Penetration test - critical bugs | 0 unresolved | Penetration Test |
| AC-SEC-004 | Penetration test - high bugs | 0 unresolved | Penetration Test |
| AC-SEC-005 | Penetration test - low bugs | 90% resolved | Penetration Test |
| AC-SEC-006 | Environment isolation | No lateral movement | Penetration Test |
| AC-SEC-007 | Network segmentation | Verified if shared subscription | Automated |

---

## 13. Testing Exit Criteria

### Functional Testing (Deidentified Data)
| ID | Criterion | Threshold |
|----|-----------|-----------|
| AC-EXIT-F01 | Test cases executed | 100% |
| AC-EXIT-F02 | Critical bugs resolved | 100% |
| AC-EXIT-F03 | High bugs resolved | 100% |
| AC-EXIT-F04 | Low bugs resolved | 85% |
| AC-EXIT-F05 | Field option consistency | 100% |
| AC-EXIT-F06 | Data transformations accurate | 100% |
| AC-EXIT-F07 | Derived fields accurate | 100% |

### End-to-End Testing (Production Data)
| ID | Criterion | Threshold |
|----|-----------|-----------|
| AC-EXIT-E01 | Test cases executed | 100% |
| AC-EXIT-E02 | Critical bugs resolved | 100% |
| AC-EXIT-E03 | High bugs resolved | 100% |
| AC-EXIT-E04 | Low bugs resolved | 90% |
| AC-EXIT-E05 | Load time meets specifications | 100% |
| AC-EXIT-E06 | Nightly data processing | <3 hours |
| AC-EXIT-E07 | Field consistency | 100% |
| AC-EXIT-E08 | Data transformations | 100% |
| AC-EXIT-E09 | DWID match | 100% |
| AC-EXIT-E10 | Derived fields match | 100% |
| AC-EXIT-E11 | Same-to-same variables | 100% |

### User Acceptance Testing (Production Data)
| ID | Criterion | Threshold |
|----|-----------|-----------|
| AC-EXIT-U01 | Test cases executed | 100% |
| AC-EXIT-U02 | Critical bugs resolved | 100% |
| AC-EXIT-U03 | High bugs resolved | 100% |
| AC-EXIT-U04 | Low bugs resolved | 90% |
| AC-EXIT-U05 | E2E data validation criteria met | 100% |
| AC-EXIT-U06 | Data comparison accurate | 100% |

### Performance Testing
| ID | Criterion | Threshold |
|----|-----------|-----------|
| AC-EXIT-P01 | Critical bugs resolved | 100% |
| AC-EXIT-P02 | High bugs resolved | 100% |
| AC-EXIT-P03 | Low bugs resolved | 90% |

### Penetration Testing
| ID | Criterion | Threshold |
|----|-----------|-----------|
| AC-EXIT-PT01 | Critical bugs resolved | 100% |
| AC-EXIT-PT02 | High bugs resolved | 100% |
| AC-EXIT-PT03 | Low bugs resolved | 90% |

---

## 14. Code Quality Criteria

| ID | Criterion | Threshold | Testable By |
|----|-----------|-----------|-------------|
| AC-CODE-001 | Unit test coverage | ≥70% | Automated |
| AC-CODE-002 | Automated tests in CI/CD pipeline | Integrated | Automated |
| AC-CODE-003 | Code review completed | All code | Manual |
| AC-CODE-004 | Automated code scans passed | All issues addressed | Automated |

---

## Summary Statistics

| Category | Count |
|----------|-------|
| Total Acceptance Criteria | 156 |
| Automated Testable | 128 |
| Manual/Audit Testable | 28 |
| Performance Criteria | 6 |
| Security Criteria | 7 |
| Exit Criteria | 24 |
| Code Quality Criteria | 4 |
