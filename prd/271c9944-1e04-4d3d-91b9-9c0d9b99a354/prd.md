# Product Requirements Document (PRD)
## Next Generation Patient Registry (NGR) - Cystic Fibrosis Foundation

**Document Version:** 1.0  
**Date:** 2025  
**Project:** NGR - Next Generation Registry  
**Client:** Cystic Fibrosis Foundation (CFF)

---

## 1. Executive Summary

### 1.1 Problem Statement
The Cystic Fibrosis Foundation operates a Patient Registry that has been collecting clinical data since the 1960s, representing approximately 80% of CF patients in the United States (~50,000+ unique individuals). The current platform (PortCF) faces security vulnerabilities, technology deprecation risks, and lacks the flexibility to adapt to the evolving CF care landscape, including new CFTR modulator therapies, reduced in-person visits, and emerging data collection needs.

### 1.2 Solution Overview
The Next Generation Registry (NGR) is a re-platforming initiative to migrate the existing PortCF data entry platform to a modern, secure, and extensible solution. The MVP focuses on replacing current functionality while laying the foundation for future capabilities including EMR integration, patient portals, and wearable data collection.

### 1.3 Target Users
- **Primary:** Care Center Network staff (~3,000+ specialized CF physicians, nurses, and clinical team members across 136 care centers)
- **Secondary:** CF Foundation internal staff (administrators, data analysts, researchers)
- **Future:** Patients and caregivers (out of scope for MVP)

### 1.4 Key Benefits
- Mitigate security and technology deprecation risks
- Maintain continuity of critical registry operations
- Enable future integration with EMRs, HIEs, and patient-facing applications
- Improve data entry workflows with high-impact updates
- Ensure compliance with modern security standards (SOC 2 Type 2, HITRUST, or ISO 27001)

---

## 2. Business Goals

### 2.1 Objectives and Key Results (OKRs)

**Objective 1:** Successfully migrate PortCF to NGR without data loss or service disruption
- KR1: 100% of historical data migrated with verified integrity
- KR2: Zero critical/high bugs at launch
- KR3: All 136 care centers operational on NGR within launch window

**Objective 2:** Enhance security posture and compliance
- KR1: Achieve third-party security validation (SOC 2 Type 2, HITRUST, or ISO 27001)
- KR2: Implement SSO integration for all users
- KR3: Pass penetration testing with all critical/high vulnerabilities resolved

**Objective 3:** Maintain or improve user productivity
- KR1: Form load times meet or exceed current PortCF performance
- KR2: Nightly data processing completes in <3 hours
- KR3: User acceptance testing achieves 100% data comparison accuracy

**Objective 4:** Enable future extensibility
- KR1: Architecture supports programmatic data import/export
- KR2: Platform can accommodate EMR integration without major refactoring
- KR3: Design allows for patient portal addition in future phases

---

## 3. Scope

### 3.1 In Scope (MVP)

| ID | Feature Area | Description |
|----|--------------|-------------|
| SCOPE-001 | SSO Integration | Single Sign-On integration for authentication |
| SCOPE-002 | Role-Based Access Control | User management for care programs and Foundation |
| SCOPE-003 | Patient Roster Management | Add, transfer, merge, remove patients from program rosters |
| SCOPE-004 | Case Report Forms (eCRFs) | Electronic forms with business rules for ownership, sharing, and editing |
| SCOPE-005 | Dynamic Report Builder | Tool to produce patient lists with custom criteria |
| SCOPE-006 | Standard Reports | Pre-built patient list reports for care programs and Foundation |
| SCOPE-007 | CSV Upload Utility | Pre-fill eCRFs from EMR data via file upload |
| SCOPE-008 | Content Management | Foundation can share/edit documentation and announcements |
| SCOPE-009 | Data Migration | Historical data migration from PortCF to NGR |
| SCOPE-010 | Data Warehouse Feed | Data feed from NGR to existing Foundation data warehouse |
| SCOPE-011 | Foundation User Training | Ongoing training for internal Foundation users |

### 3.2 Out of Scope

| ID | Feature Area | Rationale |
|----|--------------|-----------|
| OOS-001 | Care Program User Training | Explicitly excluded; CFF will handle separately |
| OOS-002 | Patient Portal | Future opportunity; architecture must enable |
| OOS-003 | EMR Integration (real-time) | Future opportunity; CSV upload is MVP alternative |
| OOS-004 | HIE/QHIN Integration | Future opportunity |
| OOS-005 | Wearable Data Collection | Future opportunity |
| OOS-006 | Data Warehouse Re-platforming | Retain existing unless major synergies identified |
| OOS-007 | CFSmartReports Replacement | Retain existing reporting solution |
| OOS-008 | Real-time Reporting | Future opportunity |
| OOS-009 | Custom Project Deployment | Future opportunity for subset of programs |

---

## 4. Functional Requirements

### 4.1 Authentication & Authorization

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-F001 | System shall integrate with SSO provider for user authentication | MUST | Security requirement |
| REQ-F002 | System shall implement role-based access control (RBAC) | MUST | Different permissions for care programs vs Foundation |
| REQ-F003 | Care programs shall manage their own users within the system | MUST | Self-service user management |
| REQ-F004 | Foundation shall have administrative user management capabilities | MUST | Cross-program administration |
| REQ-F005 | System shall support multiple user roles with configurable permissions | MUST | Per Appendix A requirements |

### 4.2 Patient Roster Management

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-F006 | Users shall add new patients to the registry | MUST | New patient enrollment |
| REQ-F007 | Users shall transfer patients between care programs | MUST | Patient mobility support |
| REQ-F008 | Users shall merge duplicate patient records | MUST | Data quality management |
| REQ-F009 | Users shall remove patients from their program roster | MUST | Roster maintenance |
| REQ-F010 | System shall maintain master patient index integrity | MUST | Critical for data quality |

### 4.3 Case Report Forms (eCRFs)

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-F011 | System shall render eCRFs based on machine-readable specifications | MUST | Per Appendix C format |
| REQ-F012 | eCRFs shall enforce business rules for ownership by care programs | MUST | Data governance |
| REQ-F013 | eCRFs shall support sharing rules between care programs | MUST | Collaborative care |
| REQ-F014 | eCRFs shall enforce edit permissions based on user role and timing | MUST | Data integrity |
| REQ-F015 | System shall support field-level validation rules | MUST | Data quality |
| REQ-F016 | System shall support conditional logic for field display | MUST | Dynamic forms |
| REQ-F017 | System shall support derived/calculated fields | MUST | Automated calculations |
| REQ-F018 | System shall auto-save form data to prevent data loss | SHOULD | User experience |
| REQ-F019 | System shall support form versioning | SHOULD | Audit trail |

### 4.4 Data Import/Export

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-F020 | System shall accept CSV file uploads to pre-fill eCRFs | MUST | EMR data integration |
| REQ-F021 | System shall validate uploaded CSV data against form specifications | MUST | Data quality |
| REQ-F022 | System shall provide data feed to Foundation data warehouse | MUST | Nightly processing |
| REQ-F023 | Data feed shall complete nightly processing in <3 hours | MUST | Performance requirement |
| REQ-F024 | System shall support programmatic data import/export via API | SHOULD | Future extensibility |

### 4.5 Reporting

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-F025 | System shall provide dynamic report builder for patient lists | MUST | Custom queries |
| REQ-F026 | System shall provide standard pre-built reports | MUST | Common use cases |
| REQ-F027 | Reports shall be accessible to care programs (program-specific data) | MUST | Data access control |
| REQ-F028 | Reports shall be accessible to Foundation users (cross-program data) | MUST | Administrative access |
| REQ-F029 | Reports shall be exportable in standard formats | SHOULD | Data portability |

### 4.6 Content Management

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-F030 | Foundation shall publish documentation via the interface | MUST | User guidance |
| REQ-F031 | Foundation shall publish announcements to care centers | MUST | Communication |
| REQ-F032 | Foundation shall edit published content without developer intervention | MUST | Self-service |

### 4.7 Data Migration

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-F033 | All historical patient data shall be migrated from PortCF | MUST | Data continuity |
| REQ-F034 | Migration shall achieve 100% DWID (Data Warehouse ID) match | MUST | Identity preservation |
| REQ-F035 | Migration shall achieve 100% accuracy for same-to-same variables | MUST | Data integrity |
| REQ-F036 | Migration approach shall minimize system downtime | MUST | Operational continuity |
| REQ-F037 | Migration shall support parallel operation during transition | SHOULD | Launch flexibility |

---

## 5. Non-Functional Requirements

### 5.1 Performance

| ID | Requirement | Priority | Metric |
|----|-------------|----------|--------|
| REQ-NF001 | Page/form load time shall meet specifications | MUST | Per Appendix A |
| REQ-NF002 | Nightly data processing shall complete in <3 hours | MUST | Processing window |
| REQ-NF003 | System shall support concurrent users across 136 care centers | MUST | ~3,000+ potential users |
| REQ-NF004 | Web servers shall be load balanced | MUST | Fault tolerance |

### 5.2 Security

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-NF005 | System shall protect PHI confidentiality, integrity, availability | MUST | HIPAA compliance |
| REQ-NF006 | System shall achieve third-party security validation | MUST | SOC 2 Type 2, HITRUST, or ISO 27001 |
| REQ-NF007 | Environments shall be fully isolated from one another | MUST | Security architecture |
| REQ-NF008 | Security controls shall prevent lateral movement between environments | MUST | Breach containment |
| REQ-NF009 | Network segmentation shall be implemented if environments share cloud subscription | MUST | Environment isolation |
| REQ-NF010 | System shall pass penetration testing with all critical/high issues resolved | MUST | Security validation |
| REQ-NF011 | System shall implement secure development practices | MUST | Per Appendix E |

### 5.3 Scalability

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-NF012 | System shall support ~35,000 active patients | MUST | Current registry size |
| REQ-NF013 | System shall support ~4 million encounters | MUST | Historical data |
| REQ-NF014 | System shall accommodate growth of ~1,000 patients/year | MUST | Projected growth |
| REQ-NF015 | System shall accommodate growth of ~120,000 encounters/year | MUST | Projected growth |
| REQ-NF016 | System shall support ~1 TB data volume | MUST | Current estimate |

### 5.4 Availability & Reliability

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-NF017 | System shall provide fault tolerance via load balancing | MUST | High availability |
| REQ-NF018 | System shall support secure, uninterrupted operations | MUST | Business continuity |

### 5.5 Maintainability

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-NF019 | Code shall achieve minimum 70% unit test coverage | MUST | Quality assurance |
| REQ-NF020 | Automated tests shall be integrated into build/deployment pipeline | MUST | CI/CD |
| REQ-NF021 | Architecture shall support future extensibility | MUST | EMR, patient portal, etc. |
| REQ-NF022 | System shall support programmatic data import/export | MUST | Interoperability |

### 5.6 Compliance

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| REQ-NF023 | System shall comply with HIPAA requirements | MUST | PHI handling |
| REQ-NF024 | System shall comply with requirements in Appendix E | MUST | Security/Privacy/Compliance |

---

## 6. Assumptions

| ID | Assumption | Impact if Invalid |
|----|------------|-------------------|
| ASM-001 | CFF will provide machine-readable form specifications for all eCRFs | Delays in form development |
| ASM-002 | CFF will provide deidentified data for development/testing environments | Testing delays |
| ASM-003 | CFF will provide production data for E2E and UAT testing | Testing scope reduction |
| ASM-004 | CFF will provide cross-functional team including technical resources, BAs, testers, PO, PM | Resource constraints |
| ASM-005 | CFF will provide pilot group of care programs for UAT | Limited user validation |
| ASM-006 | CFF Azure DevOps instance will be available for work tracking | Tool procurement needed |
| ASM-007 | CFF Microsoft Teams will be available for collaboration | Alternative tools needed |
| ASM-008 | Existing data warehouse will remain operational and accept data feeds | Integration redesign |
| ASM-009 | SSO provider is already in place at CFF | SSO implementation scope increase |
| ASM-010 | Appendix A contains detailed functional/technical requirements | Requirements gaps |
| ASM-011 | Appendix E contains complete security/privacy/compliance requirements | Compliance gaps |
| ASM-012 | Project commences Q2 2026 | Timeline adjustment |
| ASM-013 | Firm-Fixed Price contract structure is acceptable | Commercial negotiation |

---

## 7. Dependencies

### 7.1 External Systems

| ID | System | Dependency Type | Notes |
|----|--------|-----------------|-------|
| DEP-001 | CFF SSO Provider | Integration | Authentication |
| DEP-002 | CFF Data Warehouse | Integration | Nightly data feed |
| DEP-003 | PortCF (Legacy) | Data Source | Migration source |
| DEP-004 | Azure DevOps | Tooling | Work management |
| DEP-005 | Microsoft Teams | Tooling | Collaboration |

### 7.2 Internal Dependencies

| ID | Dependency | Notes |
|----|------------|-------|
| DEP-006 | CFF Governance Document | Required before kickoff |
| DEP-007 | CFF Decision Rights Document | Required before kickoff |
| DEP-008 | Machine-readable Form Specifications | Required for eCRF development |
| DEP-009 | Deidentified Test Data | Required for development |
| DEP-010 | Production Data Access | Required for E2E and UAT |
| DEP-011 | Pilot Care Program Availability | Required for UAT |
| DEP-012 | Care Program User Training (OOS) | Impacts launch timeline |

### 7.3 Third-Party Services

| ID | Service | Purpose |
|----|---------|---------|
| DEP-013 | Security Auditor | SOC 2 Type 2, HITRUST, or ISO 27001 validation |
| DEP-014 | Penetration Testing Provider | Security validation |

---

## 8. Constraints

| ID | Constraint | Impact |
|----|------------|--------|
| CON-001 | Firm-Fixed Price contract with milestone-based payments | Risk allocation |
| CON-002 | Project commencement Q2 2026 | Timeline fixed |
| CON-003 | Target availability of major components in 2027 | Scope/quality tradeoffs |
| CON-004 | Must use CFF Azure DevOps for work tracking | Tool standardization |
| CON-005 | Must use CFF Microsoft Teams for collaboration | Tool standardization |
| CON-006 | Code must be submitted to agreed repository | IP/access considerations |
| CON-007 | Two-week maximum sprint duration | Agile cadence |
| CON-008 | Launch strategy must minimize data quality compromise | Migration complexity |
| CON-009 | Launch strategy must minimize burden on data entry personnel | Change management |

---

## 9. Risks

| ID | Risk | Likelihood | Impact | Mitigation |
|----|------|------------|--------|------------|
| RSK-001 | Requirements in Appendix A are incomplete or ambiguous | Medium | High | Early requirements clarification sessions |
| RSK-002 | Data migration complexity underestimated | Medium | High | Early migration POC, parallel operation |
| RSK-003 | Care program adoption resistance | Medium | Medium | Pilot program engagement, training |
| RSK-004 | Security certification delays | Low | High | Early engagement with auditors |
| RSK-005 | Integration with existing data warehouse issues | Medium | Medium | Early integration testing |
| RSK-006 | Parallel operation during launch creates data integrity issues | Medium | High | Clear data governance, catch-up migration |
| RSK-007 | Form specification complexity exceeds estimates | Medium | Medium | Iterative form delivery, early feedback |

---

## 10. Success Criteria

| ID | Criterion | Measurement |
|----|-----------|-------------|
| SC-001 | All critical/high bugs resolved before launch | Bug tracking |
| SC-002 | 90% of low bugs resolved before launch | Bug tracking |
| SC-003 | 100% data migration accuracy | Data comparison |
| SC-004 | 100% DWID match | Data validation |
| SC-005 | Nightly processing <3 hours | Performance monitoring |
| SC-006 | Third-party security validation achieved | Certification |
| SC-007 | All 136 care centers operational | Deployment tracking |
| SC-008 | Foundation internal users trained | Training completion |

---

## 11. Milestones (High-Level)

| Milestone | Target | Notes |
|-----------|--------|-------|
| Project Kickoff & Planning | Q2 2026 | Contract finalization |
| Design & Architecture | Q2-Q3 2026 | Architecture approval |
| Iterative Configuration & Development | Q3 2026 - Q3 2027 | Incremental delivery |
| Foundation Internal User Training | Ongoing | Aligned with feature delivery |
| End-to-End Testing | Q3 2027 | Production data |
| User Acceptance Testing | Q3-Q4 2027 | Pilot care programs |
| Performance Testing | Q4 2027 | Milestone checkpoints |
| Penetration Testing | Q4 2027 | Milestone checkpoints |
| Care Program User Training | Q4 2027 - Q1 2028 | Out of scope but dependency |
| Launch | Q1 2028 | Calendar year alignment preferred |

---

## Appendix References

- **Appendix A:** Detailed Functional and Technical Requirements
- **Appendix B:** Requirements Matrix (vendor completion)
- **Appendix C:** Sample Machine-Readable Form Specification (Demographics eCRF)
- **Appendix E:** Security, Privacy, and Compliance Requirements
- **Appendix G:** Foundation Roles, Skills, and Resource Allocation

*Note: Appendix contents were referenced but not provided in full in the RFP document. These must be obtained for complete requirements analysis.*
