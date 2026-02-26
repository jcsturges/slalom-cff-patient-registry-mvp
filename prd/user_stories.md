# User Stories
## Next Generation Patient Registry (NGR) - Cystic Fibrosis Foundation

**Document Version:** 1.0  
**Date:** 2025  
**Project:** NGR - Next Generation Registry

---

## Personas

| Persona | Description |
|---------|-------------|
| Care Program User | Clinical staff (physicians, nurses, coordinators) at CF care centers who enter and manage patient data |
| Care Program Admin | Administrative user at a care center responsible for managing program users and settings |
| Foundation Admin | CFF staff with cross-program administrative access |
| Foundation Analyst | CFF staff who access reports and data for research/analysis |
| System | Automated processes and integrations |

---

## Epic 1: Authentication & User Management

### Story US-001: SSO Authentication
**As a** Care Program User, **I want to** log in using my organization's single sign-on credentials **so that** I don't need to manage separate credentials for the registry.

#### Acceptance Criteria (BDD)
- GIVEN I am an authorized user WHEN I navigate to the NGR login page THEN I am redirected to the SSO provider
- GIVEN I have valid SSO credentials WHEN I authenticate successfully THEN I am redirected to the NGR dashboard
- GIVEN I have invalid SSO credentials WHEN I attempt to authenticate THEN I receive an appropriate error message
- GIVEN my SSO session has expired WHEN I attempt to access NGR THEN I am prompted to re-authenticate

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** DEP-001 (SSO Provider)

---

### Story US-002: Care Program User Management
**As a** Care Program Admin, **I want to** manage users within my care program **so that** I can control who has access to our patient data.

#### Acceptance Criteria (BDD)
- GIVEN I am a Care Program Admin WHEN I access user management THEN I see only users associated with my program
- GIVEN I am a Care Program Admin WHEN I add a new user THEN I can assign them a role within my program
- GIVEN I am a Care Program Admin WHEN I deactivate a user THEN they can no longer access the system
- GIVEN I am a Care Program Admin WHEN I modify a user's role THEN their permissions are updated immediately

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-001

---

### Story US-003: Foundation User Management
**As a** Foundation Admin, **I want to** manage users across all care programs **so that** I can maintain system-wide access control.

#### Acceptance Criteria (BDD)
- GIVEN I am a Foundation Admin WHEN I access user management THEN I see users across all care programs
- GIVEN I am a Foundation Admin WHEN I search for a user THEN I can filter by care program, role, or status
- GIVEN I am a Foundation Admin WHEN I modify a user's access THEN the change is logged for audit purposes
- GIVEN I am a Foundation Admin WHEN I view a user's profile THEN I see their role history and access log

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-001

---

### Story US-004: Role-Based Access Control
**As a** System, **I want to** enforce role-based permissions **so that** users can only access data and functions appropriate to their role.

#### Acceptance Criteria (BDD)
- GIVEN a user has a specific role WHEN they attempt to access a restricted function THEN access is denied with appropriate message
- GIVEN a user has a specific role WHEN they access the system THEN they see only menu items and data permitted for their role
- GIVEN a Care Program User WHEN they access patient data THEN they see only patients in their program roster
- GIVEN a Foundation Admin WHEN they access patient data THEN they can view patients across all programs

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-001, US-002, US-003

---

## Epic 2: Patient Roster Management

### Story US-005: Add New Patient
**As a** Care Program User, **I want to** add a new patient to the registry **so that** I can begin tracking their clinical data.

#### Acceptance Criteria (BDD)
- GIVEN I am a Care Program User WHEN I initiate new patient enrollment THEN I am presented with required demographic fields
- GIVEN I enter valid patient information WHEN I submit the enrollment THEN the patient is added to my program roster
- GIVEN I enter invalid or incomplete information WHEN I submit the enrollment THEN I receive validation errors
- GIVEN a patient is successfully enrolled WHEN I view my roster THEN the new patient appears in the list

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-004

---

### Story US-006: Transfer Patient Between Programs
**As a** Care Program User, **I want to** transfer a patient to another care program **so that** their care continuity is maintained when they change providers.

#### Acceptance Criteria (BDD)
- GIVEN I am a Care Program User WHEN I initiate a patient transfer THEN I can select the destination program
- GIVEN I initiate a transfer WHEN the receiving program accepts THEN the patient appears on their roster
- GIVEN a transfer is completed WHEN I view my roster THEN the patient no longer appears
- GIVEN a transfer is completed WHEN the receiving program views the patient THEN they see the complete historical data

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-005

---

### Story US-007: Merge Duplicate Patient Records
**As a** Care Program User, **I want to** merge duplicate patient records **so that** data integrity is maintained.

#### Acceptance Criteria (BDD)
- GIVEN I identify duplicate records WHEN I initiate a merge THEN I can select which record is the primary
- GIVEN I am merging records WHEN I review the merge preview THEN I see how data will be consolidated
- GIVEN I confirm a merge WHEN the merge completes THEN all historical data is preserved under the primary record
- GIVEN a merge is completed WHEN I search for the merged patient THEN only the primary record appears

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-005

---

### Story US-008: Remove Patient from Roster
**As a** Care Program User, **I want to** remove a patient from my program roster **so that** my active patient list is accurate.

#### Acceptance Criteria (BDD)
- GIVEN I am a Care Program User WHEN I remove a patient THEN I must provide a reason (transfer, deceased, withdrawn, etc.)
- GIVEN I remove a patient WHEN I view my roster THEN the patient no longer appears in active patients
- GIVEN a patient is removed WHEN I search with appropriate filters THEN I can still find their historical record
- GIVEN a patient is removed WHEN the removal is logged THEN the audit trail captures the action and reason

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-005

---

### Story US-009: Search and Filter Patient Roster
**As a** Care Program User, **I want to** search and filter my patient roster **so that** I can quickly find specific patients.

#### Acceptance Criteria (BDD)
- GIVEN I am viewing my roster WHEN I enter search criteria THEN results are filtered in real-time
- GIVEN I am searching WHEN I use multiple filters THEN they are applied with AND logic
- GIVEN I find a patient WHEN I click on their record THEN I am taken to their patient summary
- GIVEN no results match my search WHEN the search completes THEN I see an appropriate message

**Priority:** MUST  
**Estimate:** S  
**Dependencies:** US-005

---

## Epic 3: Case Report Forms (eCRFs)

### Story US-010: View and Complete eCRF
**As a** Care Program User, **I want to** view and complete electronic case report forms **so that** I can record patient clinical data.

#### Acceptance Criteria (BDD)
- GIVEN I select a patient WHEN I access their forms THEN I see available eCRFs for that patient
- GIVEN I open an eCRF WHEN the form loads THEN all fields are rendered according to the form specification
- GIVEN I enter data in a field WHEN I move to the next field THEN field-level validation is applied
- GIVEN I complete a form WHEN I save THEN the data is persisted and a confirmation is displayed

**Priority:** MUST  
**Estimate:** XL  
**Dependencies:** US-005, DEP-008 (Form Specifications)

---

### Story US-011: eCRF Conditional Logic
**As a** Care Program User, **I want to** see form fields dynamically show/hide based on my inputs **so that** I only see relevant fields.

#### Acceptance Criteria (BDD)
- GIVEN a field has conditional display rules WHEN the condition is met THEN the field becomes visible
- GIVEN a field has conditional display rules WHEN the condition is not met THEN the field is hidden
- GIVEN I change a value that triggers conditional logic WHEN the form updates THEN dependent fields show/hide appropriately
- GIVEN a hidden field had data WHEN it becomes hidden THEN the data handling follows business rules (clear or preserve)

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-010

---

### Story US-012: eCRF Validation Rules
**As a** Care Program User, **I want to** receive validation feedback when entering data **so that** I can correct errors before submission.

#### Acceptance Criteria (BDD)
- GIVEN a field has validation rules WHEN I enter invalid data THEN I see an inline error message
- GIVEN a form has required fields WHEN I attempt to save with missing required data THEN I see which fields need attention
- GIVEN validation errors exist WHEN I correct the data THEN the error message is removed
- GIVEN all validations pass WHEN I save the form THEN the save succeeds

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-010

---

### Story US-013: eCRF Derived Fields
**As a** Care Program User, **I want to** see calculated/derived fields auto-populate **so that** I don't need to manually calculate values.

#### Acceptance Criteria (BDD)
- GIVEN a field is derived from other fields WHEN I enter the source values THEN the derived field auto-calculates
- GIVEN a derived field exists WHEN I view the form THEN the derived field is read-only
- GIVEN source values change WHEN I update them THEN the derived field recalculates
- GIVEN derived field calculation fails WHEN an error occurs THEN an appropriate message is displayed

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-010

---

### Story US-014: eCRF Ownership and Sharing Rules
**As a** Care Program User, **I want to** understand which forms I can edit vs. view-only **so that** I know my data entry permissions.

#### Acceptance Criteria (BDD)
- GIVEN a form is owned by my program WHEN I access it THEN I can edit the form
- GIVEN a form is owned by another program WHEN I access it THEN I can only view the form
- GIVEN a form has sharing rules WHEN I access a shared form THEN my permissions follow the sharing rules
- GIVEN my edit window has expired WHEN I access a form THEN I can only view, not edit

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-010, US-004

---

### Story US-015: eCRF Auto-Save
**As a** Care Program User, **I want to** have my form data auto-saved periodically **so that** I don't lose work if my session is interrupted.

#### Acceptance Criteria (BDD)
- GIVEN I am editing a form WHEN a defined interval passes THEN my data is auto-saved as draft
- GIVEN my session is interrupted WHEN I return to the form THEN I can resume from the auto-saved draft
- GIVEN auto-save occurs WHEN the save completes THEN I see a subtle confirmation indicator
- GIVEN auto-save fails WHEN an error occurs THEN I am notified to manually save

**Priority:** SHOULD  
**Estimate:** M  
**Dependencies:** US-010

---

## Epic 4: Data Import

### Story US-016: CSV Upload for eCRF Pre-fill
**As a** Care Program User, **I want to** upload a CSV file to pre-fill eCRF data **so that** I can reduce manual data entry from our EMR.

#### Acceptance Criteria (BDD)
- GIVEN I have EMR data in CSV format WHEN I access the upload utility THEN I can select and upload my file
- GIVEN I upload a valid CSV WHEN processing completes THEN matching eCRF fields are pre-filled
- GIVEN I upload a CSV with validation errors WHEN processing completes THEN I see a report of errors
- GIVEN fields are pre-filled WHEN I review the eCRF THEN I can verify and modify the data before final save

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-010

---

### Story US-017: CSV Upload Validation
**As a** Care Program User, **I want to** receive feedback on CSV upload errors **so that** I can correct issues in my source data.

#### Acceptance Criteria (BDD)
- GIVEN I upload a CSV WHEN the file format is invalid THEN I receive a clear error message
- GIVEN I upload a CSV WHEN data validation fails THEN I see which rows/fields have issues
- GIVEN validation errors exist WHEN I review the report THEN I can download the error details
- GIVEN I correct errors WHEN I re-upload THEN previously failed records are processed

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-016

---

## Epic 5: Reporting

### Story US-018: Dynamic Report Builder
**As a** Care Program User, **I want to** build custom patient list reports **so that** I can query my patient population for specific criteria.

#### Acceptance Criteria (BDD)
- GIVEN I access the report builder WHEN I define filter criteria THEN I can select from available data fields
- GIVEN I define multiple criteria WHEN I run the report THEN results match all criteria (AND logic)
- GIVEN I run a report WHEN results are returned THEN I see a list of matching patients
- GIVEN I have report results WHEN I want to save the query THEN I can save it for future use

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-004

---

### Story US-019: Standard Reports
**As a** Care Program User, **I want to** access pre-built standard reports **so that** I can quickly get common information without building queries.

#### Acceptance Criteria (BDD)
- GIVEN I access the reports section WHEN I view available reports THEN I see a list of standard reports
- GIVEN I select a standard report WHEN I run it THEN I see results for my program's patients
- GIVEN I view report results WHEN I want to export THEN I can download in standard formats
- GIVEN I am a Foundation user WHEN I run a standard report THEN I see cross-program results

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-004

---

### Story US-020: Export Report Results
**As a** Care Program User, **I want to** export report results **so that** I can use the data in other tools.

#### Acceptance Criteria (BDD)
- GIVEN I have report results WHEN I click export THEN I can select the export format
- GIVEN I select CSV format WHEN the export completes THEN I receive a properly formatted CSV file
- GIVEN I export data WHEN the export contains PHI THEN appropriate security measures are applied
- GIVEN I export a large dataset WHEN processing takes time THEN I see a progress indicator

**Priority:** SHOULD  
**Estimate:** S  
**Dependencies:** US-018, US-019

---

## Epic 6: Content Management

### Story US-021: Publish Documentation
**As a** Foundation Admin, **I want to** publish documentation to care centers **so that** users have access to guidance and reference materials.

#### Acceptance Criteria (BDD)
- GIVEN I am a Foundation Admin WHEN I access content management THEN I can create new documentation
- GIVEN I create documentation WHEN I publish it THEN it becomes visible to care program users
- GIVEN documentation is published WHEN I need to update it THEN I can edit without developer assistance
- GIVEN I update documentation WHEN I save changes THEN the updated version is immediately available

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-004

---

### Story US-022: Publish Announcements
**As a** Foundation Admin, **I want to** publish announcements to care centers **so that** I can communicate important information.

#### Acceptance Criteria (BDD)
- GIVEN I am a Foundation Admin WHEN I create an announcement THEN I can set visibility and expiration
- GIVEN an announcement is published WHEN users log in THEN they see the announcement prominently
- GIVEN an announcement has expired WHEN the expiration date passes THEN it is no longer displayed
- GIVEN I need to edit an announcement WHEN I access it THEN I can modify content and settings

**Priority:** MUST  
**Estimate:** S  
**Dependencies:** US-004

---

### Story US-023: View Documentation and Announcements
**As a** Care Program User, **I want to** view documentation and announcements **so that** I stay informed about registry updates and guidance.

#### Acceptance Criteria (BDD)
- GIVEN I log into the system WHEN there are active announcements THEN I see them on my dashboard
- GIVEN I want to access documentation WHEN I navigate to the help section THEN I see available documents
- GIVEN I view a document WHEN I open it THEN I see the current published version
- GIVEN I have read an announcement WHEN I dismiss it THEN it doesn't show again (if dismissible)

**Priority:** MUST  
**Estimate:** S  
**Dependencies:** US-021, US-022

---

## Epic 7: Data Integration

### Story US-024: Data Warehouse Feed
**As a** System, **I want to** send data to the Foundation data warehouse nightly **so that** downstream analytics and reporting are supported.

#### Acceptance Criteria (BDD)
- GIVEN the nightly processing window begins WHEN the feed executes THEN all new/modified data is transmitted
- GIVEN the feed executes WHEN processing completes THEN it finishes within 3 hours
- GIVEN the feed completes WHEN data arrives at the warehouse THEN data transformations are 100% accurate
- GIVEN the feed fails WHEN an error occurs THEN appropriate alerts are generated

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** DEP-002 (Data Warehouse)

---

### Story US-025: Data Feed Monitoring
**As a** Foundation Admin, **I want to** monitor data feed status **so that** I can ensure data warehouse integration is functioning.

#### Acceptance Criteria (BDD)
- GIVEN I am a Foundation Admin WHEN I access system monitoring THEN I see data feed status
- GIVEN a feed has completed WHEN I view the log THEN I see record counts and processing time
- GIVEN a feed has failed WHEN I view the log THEN I see error details
- GIVEN feeds are running WHEN I need historical data THEN I can view past feed results

**Priority:** SHOULD  
**Estimate:** S  
**Dependencies:** US-024

---

## Epic 8: Data Migration

### Story US-026: Historical Data Migration
**As a** Foundation Admin, **I want to** migrate all historical data from PortCF **so that** data continuity is maintained.

#### Acceptance Criteria (BDD)
- GIVEN migration is initiated WHEN processing completes THEN all patient records are migrated
- GIVEN migration completes WHEN data is validated THEN DWID match is 100%
- GIVEN migration completes WHEN data is validated THEN same-to-same variables match 100%
- GIVEN migration completes WHEN data is validated THEN derived fields match 100%

**Priority:** MUST  
**Estimate:** XL  
**Dependencies:** DEP-003 (PortCF)

---

### Story US-027: Migration Validation
**As a** Foundation Admin, **I want to** validate migrated data **so that** I can confirm data integrity before launch.

#### Acceptance Criteria (BDD)
- GIVEN migration is complete WHEN I run validation THEN I see comparison reports
- GIVEN validation identifies discrepancies WHEN I review them THEN I can investigate root causes
- GIVEN discrepancies are resolved WHEN I re-validate THEN the issues are cleared
- GIVEN validation passes WHEN all criteria are met THEN migration is certified complete

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** US-026

---

### Story US-028: Parallel Operation Support
**As a** Care Program User, **I want to** operate in both PortCF and NGR during transition **so that** data entry continuity is maintained.

#### Acceptance Criteria (BDD)
- GIVEN parallel operation is active WHEN I enter data in PortCF THEN it is for the prior year
- GIVEN parallel operation is active WHEN I enter data in NGR THEN it is for the current year
- GIVEN parallel operation ends WHEN catch-up migration runs THEN any remaining PortCF data is migrated
- GIVEN parallel operation is active WHEN I view a patient THEN I understand which system to use

**Priority:** SHOULD  
**Estimate:** L  
**Dependencies:** US-026

---

## Epic 9: System Administration

### Story US-029: Environment Management
**As a** System, **I want to** maintain isolated environments **so that** security and data integrity are preserved.

#### Acceptance Criteria (BDD)
- GIVEN multiple environments exist WHEN they are deployed THEN each is fully isolated
- GIVEN an environment is compromised WHEN security controls are tested THEN lateral movement is prevented
- GIVEN environments share a cloud subscription WHEN network configuration is reviewed THEN segmentation is verified
- GIVEN the training environment is needed WHEN it is provisioned THEN it is a copy of production

**Priority:** MUST  
**Estimate:** L  
**Dependencies:** None

---

### Story US-030: Audit Logging
**As a** Foundation Admin, **I want to** access audit logs **so that** I can track system access and data changes.

#### Acceptance Criteria (BDD)
- GIVEN a user performs an action WHEN the action completes THEN it is logged with timestamp and user ID
- GIVEN I need to investigate an issue WHEN I search audit logs THEN I can filter by user, action, date
- GIVEN PHI is accessed WHEN the access occurs THEN it is logged for compliance
- GIVEN audit logs exist WHEN retention policies apply THEN logs are retained per requirements

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-004

---

## Epic 10: Training Support

### Story US-031: Foundation User Training Environment
**As a** Foundation Admin, **I want to** access a training environment **so that** internal users can learn the system safely.

#### Acceptance Criteria (BDD)
- GIVEN training is scheduled WHEN users access the training environment THEN it mirrors production functionality
- GIVEN the training environment exists WHEN users make changes THEN production data is not affected
- GIVEN training is complete WHEN the environment is refreshed THEN it can be reset for next session
- GIVEN training materials reference features WHEN users practice THEN the features work as documented

**Priority:** MUST  
**Estimate:** M  
**Dependencies:** US-029

---

### Story US-032: Training Documentation Access
**As a** Foundation User, **I want to** access training materials within the system **so that** I can reference guidance while working.

#### Acceptance Criteria (BDD)
- GIVEN I am learning a feature WHEN I access help THEN I see relevant training materials
- GIVEN training materials exist WHEN I search THEN I can find specific topics
- GIVEN I am on a specific screen WHEN I click contextual help THEN I see help for that screen
- GIVEN training materials are updated WHEN I access them THEN I see the current version

**Priority:** SHOULD  
**Estimate:** S  
**Dependencies:** US-021

---

## Story Dependency Map

```
US-001 (SSO)
  ├── US-002 (Care Program User Mgmt)
  ├── US-003 (Foundation User Mgmt)
  └── US-004 (RBAC)
        ├── US-005 (Add Patient)
        │     ├── US-006 (Transfer Patient)
        │     ├── US-007 (Merge Records)
        │     ├── US-008 (Remove Patient)
        │     ├── US-009 (Search Roster)
        │     └── US-010 (eCRF)
        │           ├── US-011 (Conditional Logic)
        │           ├── US-012 (Validation)
        │           ├── US-013 (Derived Fields)
        │           ├── US-014 (Ownership Rules)
        │           ├── US-015 (Auto-Save)
        │           └── US-016 (CSV Upload)
        │                 └── US-017 (CSV Validation)
        ├── US-018 (Report Builder)
        │     └── US-020 (Export)
        ├── US-019 (Standard Reports)
        │     └── US-020 (Export)
        ├── US-021 (Publish Docs)
        │     └── US-023 (View Content)
        │     └── US-032 (Training Docs)
        ├── US-022 (Announcements)
        │     └── US-023 (View Content)
        └── US-030 (Audit Logging)

US-024 (Data Feed) ── US-025 (Feed Monitoring)

US-026 (Migration)
  ├── US-027 (Migration Validation)
  └── US-028 (Parallel Operation)

US-029 (Environments) ── US-031 (Training Env)
```

---

## Summary by Priority

| Priority | Count | Story IDs |
|----------|-------|-----------|
| MUST | 27 | US-001 through US-014, US-016, US-017, US-018, US-019, US-021, US-022, US-023, US-024, US-026, US-027, US-029, US-030, US-031 |
| SHOULD | 5 | US-015, US-020, US-025, US-028, US-032 |
| COULD | 0 | - |

## Summary by Estimate

| Estimate | Count | Story IDs |
|----------|-------|-----------|
| S | 6 | US-009, US-020, US-022, US-023, US-025, US-032 |
| M | 13 | US-001, US-002, US-003, US-005, US-008, US-013, US-014, US-015, US-017, US-019, US-021, US-030, US-031 |
| L | 10 | US-004, US-006, US-007, US-011, US-012, US-016, US-018, US-024, US-027, US-028, US-029 |
| XL | 3 | US-010, US-026 |
