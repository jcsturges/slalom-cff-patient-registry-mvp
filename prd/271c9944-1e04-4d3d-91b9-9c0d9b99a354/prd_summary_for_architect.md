# Technical Architecture Summary: Next Generation Patient Registry (NGR)

## Project Overview

**Client:** Cystic Fibrosis Foundation (CFF)  
**Project:** Platform migration from legacy PortCF to modern NGR system  
**Timeline:** Q2 2026 kickoff → Q1 2028 launch  
**Contract:** Firm-Fixed Price with milestone-based payments

### Core Problem
- Legacy platform (PortCF) with security vulnerabilities and technology deprecation
- Registry tracks ~50,000 CF patients (80% of US population) since 1960s
- Current system lacks flexibility for EMR integration, patient portals, and modern data collection
- Critical need for platform modernization without disrupting operations

### Solution Scope
Re-platforming initiative focused on **feature parity MVP** while establishing foundation for future capabilities (EMR integration, patient portals, wearable data).

---

## User Base & Scale Requirements

### Users
- **Primary:** 3,000+ clinical staff across 136 care centers (physicians, nurses, clinical teams)
- **Secondary:** CFF internal staff (administrators, analysts, researchers)
- **Future:** Patients/caregivers (out of scope for MVP)

### Data Scale
- **Current:** ~35,000 active patients, ~4 million encounters, ~1 TB data
- **Growth:** ~1,000 patients/year, ~120,000 encounters/year
- **Historical:** Data from 1960s requiring complete migration with 100% integrity

---

## Critical Architectural Requirements

### 1. Authentication & Authorization (MUST-HAVE)
- **SSO Integration:** Integrate with CFF's existing SSO provider
- **RBAC:** Role-based access control with multiple configurable roles
- **Self-Service Management:** Care programs manage own users; Foundation has cross-program admin
- **Data Segregation:** Care programs access only their data; Foundation accesses cross-program data

### 2. Core Data Management

#### Patient Registry (MUST-HAVE)
- Master patient index with integrity controls
- Patient lifecycle: add, transfer (between programs), merge duplicates, remove
- DWID (Data Warehouse ID) as primary identifier - **100% preservation required**

#### Electronic Case Report Forms (eCRFs) - MUST-HAVE
- **Dynamic Form Engine:** Render from machine-readable specifications (Appendix C format)
- **Business Rules Engine:**
  - Ownership rules (by care program)
  - Sharing rules (between programs)
  - Edit permissions (role + timing based)
- **Form Capabilities:**
  - Field-level validation
  - Conditional display logic
  - Derived/calculated fields
  - Auto-save functionality
  - Form versioning for audit trail

### 3. Data Integration Architecture

#### Inbound (MUST-HAVE)
- **CSV Upload Utility:** Pre-fill eCRFs from EMR exports
- **Validation Engine:** Validate uploads against form specifications
- **API Framework (SHOULD):** Programmatic import for future EMR integration

#### Outbound (MUST-HAVE)
- **Data Warehouse Feed:** Nightly batch to existing CFF data warehouse
- **Performance Constraint:** Complete in <3 hours
- **API Framework (SHOULD):** Programmatic export capability

### 4. Reporting System (MUST-HAVE)
- **Dynamic Report Builder:** Custom patient list queries with criteria selection
- **Standard Reports:** Pre-built reports for common use cases
- **Access Control:** Program-specific vs. cross-program data access
- **Export Capabilities (SHOULD):** Standard format exports

### 5. Content Management (MUST-HAVE)
- Foundation self-service publishing (no developer intervention)
- Documentation repository
- Announcements to care centers

---

## Non-Functional Requirements

### Performance
- **Form Load Times:** Meet specifications in Appendix A (specific metrics TBD)
- **Concurrent Users:** Support 3,000+ users across 136 locations
- **Batch Processing:** <3 hour nightly data warehouse feed
- **Load Balancing:** Required for web servers

### Security & Compliance (CRITICAL)
- **PHI Protection:** HIPAA compliance mandatory
- **Security Certification:** SOC 2 Type 2, HITRUST, or ISO 27001 required
- **Environment Isolation:**
  - Dev, Test, UAT, Production fully isolated
  - No lateral movement between environments
  - Network segmentation if sharing cloud subscription
- **Penetration Testing:** All critical/high vulnerabilities resolved before launch
- **Secure Development:** Per Appendix E requirements
- **Code Quality:** Minimum 70% unit test coverage

### Availability & Reliability
- Fault-tolerant architecture with load balancing
- Secure, uninterrupted operations
- Support for potential parallel operation during migration

### Maintainability & Extensibility (CRITICAL)
- **Architecture must support future:**
  - Real-time EMR integration
  - Patient portal integration
  - HIE/QHIN connectivity
  - Wearable data collection
- **CI/CD:** Automated testing in build/deployment pipeline
- **API-First Design:** Programmatic data import/export

---

## Data Migration Requirements (CRITICAL)

### Migration Criteria
- **100% DWID Match:** All patient identifiers preserved
- **100% Accuracy:** Same-to-same variable mapping
- **Zero Data Loss:** All historical data migrated
- **Minimize Downtime:** Operational continuity essential

### Migration Strategy Considerations
- Support for parallel operation (PortCF + NGR) during transition
- Catch-up migration capability for data entered in legacy during parallel period
- Clear data governance during transition

### Testing Requirements
- Development/Test: Deidentified data from CFF
- E2E & UAT: Production data access required
- 100% data comparison accuracy validation

---

## Integration Points & Dependencies

### External Systems (MUST INTEGRATE)
1. **CFF SSO Provider** - Authentication (assumed existing)
2. **CFF Data Warehouse** - Nightly data feed (existing system remains)
3. **PortCF** - Legacy data source for migration
4. **Azure DevOps** - CFF standard for work tracking
5. **Microsoft Teams** - CFF standard for collaboration

### Required from CFF Before Development
- Machine-readable form specifications for all eCRFs (Appendix C format)
- Deidentified test data
- Production data access (for E2E/UAT)
- Governance document
- Decision rights document
- Appendix A (detailed functional/technical requirements)
- Appendix E (security/privacy/compliance requirements)

### Third-Party Services
- Security auditor (SOC 2/HITRUST/ISO certification)
- Penetration testing provider

---

## Technical Constraints

### Mandatory
- Two-week maximum sprint duration
- Code submitted to CFF-agreed repository
- Azure DevOps for work tracking
- Microsoft Teams for collaboration
- CFF provides cross-functional team (technical resources, BAs, testers, PO, PM)

### Architecture Constraints
- Must NOT replace existing data warehouse (retain unless major synergies)
- Must NOT replace CFSmartReports (existing reporting solution)
- Environment isolation with security controls

---

## Out of Scope (But Architect For)

### Future Capabilities to Enable
- Real-time EMR integration (MVP uses CSV upload)
- Patient portal
- HIE/QHIN integration
- Wearable data collection
- Real-time reporting
- Custom project deployment for program subsets

### Explicitly Excluded
- Care program user training (CFF handles separately)
- Data warehouse re-platforming

---

## Success Metrics for Architecture

### Launch Criteria
- All critical/high bugs resolved
- 90% of low bugs resolved
- Third-party security validation achieved
- All 136 care centers operational
- Nightly processing <3 hours consistently

### Data Quality Gates
- 100% DWID match validation
- 100% data migration accuracy
- No data loss during migration

### Performance Gates
- Page/form load times meet specifications (Appendix A)
- Support concurrent users across all 136 centers
- Fault tolerance validated through load balancing

---

## High-Risk Areas Requiring Architecture Focus

### Risk Mitigation Priorities
1. **Data Migration Complexity** - Early POC, parallel operation support, catch-up mechanisms
2. **Form Engine Complexity** - Machine-readable spec interpretation, business rules engine scalability
3. **Data Warehouse Integration** - Early integration testing, nightly processing performance
4. **Security Certification** - Early auditor engagement, environment isolation validation
5. **Parallel Operation Data Integrity** - Clear governance, conflict resolution, catch-up procedures

---

## Architecture Decision Requirements

### Critical Decisions Needed
1. **Form Engine:** Build vs. buy vs. configure (must support Appendix C format)
2. **Cloud Platform:** Likely Azure (CFF tooling suggests Microsoft ecosystem)
3. **Database:** Must handle ~1TB with growth, complex relationships, audit trail
4. **API Strategy:** RESTful vs. GraphQL for future extensibility
5. **Batch Processing:** Architecture for <3 hour nightly processing at scale
6. **Environment Strategy:** Subscription isolation vs. network segmentation
7. **Migration Approach:** Big bang vs. phased vs. parallel operation

### Reference Documents Required
- **Appendix A:** Detailed functional/technical requirements (including performance specs)
- **Appendix C:** Machine-readable form specification format
- **Appendix E:** Complete security/privacy/compliance requirements
- **Appendix G:** Foundation roles and resources

---

## Timeline Context
- **Kickoff:** Q2 2026
- **Development:** Q3 2026 - Q3 2027 (iterative)
- **Testing:** Q3-Q4 2027 (E2E, UAT, performance, penetration)
- **Launch:** Q1 2028 (calendar year alignment preferred)
- **Foundation Training:** Ongoing during development
- **Care Program Training:** Q4 2027 - Q1 2028 (out of scope but launch dependency)
