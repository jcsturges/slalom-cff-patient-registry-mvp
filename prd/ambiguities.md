# Ambiguities and Open Questions
## Next Generation Patient Registry (NGR) - Cystic Fibrosis Foundation

**Document Version:** 1.0  
**Date:** 2025  
**Project:** NGR - Next Generation Registry

---

## Overview

This document identifies requirements that are unclear, contradictory, or missing from the RFP. Each ambiguity includes the source text, analysis, assumed interpretation, alternatives, and recommended clarification questions.

---

## AMB-001: Appendix Contents Not Provided

### Ambiguous Text
> "Detailed functional and technical requirements on this scope are provided in Appendix A."
> "Security, Privacy, and Compliance requirements are included as Appendix E."
> "For a list of Foundation roles, skills and resource project time allocation refer to Appendix G."

### Why It's Ambiguous
The RFP references multiple appendices (A, B, C, E, G) that contain critical detailed requirements, but only a sample form specification (Appendix C - Demographics eCRF) is mentioned as available. The actual appendix contents were not included in the provided RFP document.

### Assumed Interpretation
The appendices exist and will be provided to selected vendors. Requirements analysis is based on the main RFP body, with the understanding that appendices will provide additional detail.

### Alternative Interpretations
1. Appendices are still being developed
2. Appendices are only provided after vendor selection
3. Appendices are available upon request during RFP response period

### Recommended Clarification
**Q1:** Can CFF provide Appendices A, B, C, E, and G for complete requirements analysis?
**Q2:** Are there additional appendices (D, F) that were not referenced?
**Q3:** What is the timeline for appendix availability?

---

## AMB-002: SSO Provider Specification

### Ambiguous Text
> "SSO integration"

### Why It's Ambiguous
The RFP requires SSO integration but does not specify:
- Which SSO provider/protocol (SAML, OAuth 2.0, OIDC, Azure AD, Okta, etc.)
- Whether CFF has an existing SSO infrastructure
- Multi-factor authentication requirements
- Federation requirements for care center users

### Assumed Interpretation
CFF has an existing SSO provider (likely Azure AD given Microsoft Teams/DevOps usage) that supports standard protocols (SAML 2.0 or OIDC).

### Alternative Interpretations
1. SSO provider selection is part of the project scope
2. Each care center may have its own identity provider requiring federation
3. CFF will implement a new SSO solution in parallel

### Recommended Clarification
**Q1:** What SSO provider and protocol does CFF currently use?
**Q2:** Do care center users authenticate through CFF's SSO or their own organization's identity provider?
**Q3:** Is MFA required, and if so, what methods are acceptable?

---

## AMB-003: Form Specification Completeness

### Ambiguous Text
> "CFF will also be able to provide machine readable files for form specifications. A sample form specification file is included as Appendix C."

### Why It's Ambiguous
- Only a sample (Demographics eCRF) is mentioned
- Total number of forms is not specified
- Complexity and variation across forms is unknown
- Whether all forms have machine-readable specifications is unclear

### Assumed Interpretation
CFF will provide machine-readable specifications for all eCRFs in a consistent format similar to the Demographics sample. The number of forms is estimated based on "hundreds of fields" mentioned in the Registry description.

### Alternative Interpretations
1. Only some forms have machine-readable specifications; others require manual analysis
2. Form specifications are incomplete and require collaborative refinement
3. New forms may need to be designed as part of the project

### Recommended Clarification
**Q1:** How many eCRFs are in scope for the MVP?
**Q2:** Are machine-readable specifications available for all forms?
**Q3:** What is the average complexity (field count, validation rules, conditional logic) per form?
**Q4:** Are there forms that require redesign vs. direct migration?

---

## AMB-004: Performance Specifications

### Ambiguous Text
> "Load time meets specifications" (referenced in exit criteria)
> "Page/form load time shall meet specifications" (referenced in requirements)

### Why It's Ambiguous
Specific performance thresholds are referenced but not defined in the main RFP body. These are presumably in Appendix A.

### Assumed Interpretation
Performance specifications exist in Appendix A and define acceptable page/form load times (likely in the 2-5 second range for typical web applications).

### Alternative Interpretations
1. Performance specifications are to be proposed by the vendor
2. Performance should match or exceed current PortCF performance
3. Industry standard benchmarks apply

### Recommended Clarification
**Q1:** What are the specific page/form load time requirements?
**Q2:** What is the current PortCF performance baseline?
**Q3:** Are there different performance tiers for different operations (e.g., simple page load vs. complex report generation)?

---

## AMB-005: Data Warehouse Integration Scope

### Ambiguous Text
> "Data feed from the new platform to the Foundation's existing data warehouse."
> "Simultaneously, re-platforming the data warehouse and the complete reporting solution may be considered only if major synergies are identified."

### Why It's Ambiguous
- The exact interface/API for the data warehouse is not specified
- Whether the existing ETL processes need modification is unclear
- The data format and schema requirements are not detailed
- "Major synergies" threshold is subjective

### Assumed Interpretation
NGR will produce a data feed compatible with the existing data warehouse interface. The data warehouse itself remains unchanged, and existing ETL processes will consume the new feed with minimal modification.

### Alternative Interpretations
1. Data warehouse interface may need to be redesigned
2. ETL processes are part of the NGR scope
3. A new integration layer is needed between NGR and the data warehouse

### Recommended Clarification
**Q1:** What is the current data feed format and interface from PortCF to the data warehouse?
**Q2:** Can the existing ETL processes accept data from NGR without modification?
**Q3:** What defines "major synergies" that would trigger data warehouse re-platforming consideration?
**Q4:** Is documentation available for the current data warehouse schema and ETL processes?

---

## AMB-006: Hosting and Infrastructure

### Ambiguous Text
> "If multiple environments reside within the same cloud subscription, network segmentation must be implemented"

### Why It's Ambiguous
- Cloud provider preference is not specified (AWS, Azure, GCP, etc.)
- Whether CFF has existing cloud infrastructure is unclear
- Hosting model (vendor-managed, CFF-managed, hybrid) is not defined
- Data residency requirements are not specified

### Assumed Interpretation
Given Microsoft Teams and Azure DevOps usage, Azure is the preferred cloud platform. The vendor will propose and manage the infrastructure with CFF oversight.

### Alternative Interpretations
1. CFF has existing cloud infrastructure that must be used
2. Vendor can propose any cloud provider
3. On-premises hosting is acceptable
4. Multi-cloud or hybrid approach is required

### Recommended Clarification
**Q1:** Does CFF have a preferred cloud provider?
**Q2:** Does CFF have existing cloud infrastructure that should be leveraged?
**Q3:** What is the expected hosting model (vendor-managed SaaS, CFF-managed PaaS, etc.)?
**Q4:** Are there data residency requirements (US-only, specific regions)?

---

## AMB-007: Launch Strategy Decision Criteria

### Ambiguous Text
> "The launch strategy will be determined in consultation with the vendor based on the project timeline and an evaluation of the risk profile of the two launch options."

### Why It's Ambiguous
Two launch options are described (parallel operation vs. sequential), but:
- Decision criteria are not defined
- Risk tolerance thresholds are not specified
- Who has final decision authority is unclear
- Timeline implications of each option are not quantified

### Assumed Interpretation
CFF prefers the parallel operation approach (calendar year alignment) but will make the final decision based on project progress and risk assessment. The vendor should plan for both scenarios.

### Alternative Interpretations
1. Vendor should recommend and justify one approach
2. Decision will be made early in the project
3. Decision will be made late based on readiness assessment

### Recommended Clarification
**Q1:** What are the key decision criteria for selecting the launch strategy?
**Q2:** When must the launch strategy decision be finalized?
**Q3:** What is CFF's risk tolerance for data quality issues during transition?
**Q4:** Is there a preferred launch date or launch window?

---

## AMB-008: User Role Definitions

### Ambiguous Text
> "Roles based access control and user management for care programs and the Foundation"

### Why It's Ambiguous
- Specific roles are not enumerated
- Permission matrices are not provided
- Whether roles are configurable or fixed is unclear
- Role hierarchy is not defined

### Assumed Interpretation
Roles are defined in Appendix A and include at minimum: Care Program User, Care Program Admin, Foundation User, Foundation Admin. Roles have predefined permissions that may be configurable by Foundation Admins.

### Alternative Interpretations
1. Roles should mirror current PortCF roles exactly
2. Role design is part of the project scope
3. Roles are fully configurable by CFF

### Recommended Clarification
**Q1:** What roles exist in the current PortCF system?
**Q2:** Are there role definitions and permission matrices available?
**Q3:** Should roles be configurable or fixed?
**Q4:** Are there cross-program roles (e.g., users who work at multiple care centers)?

---

## AMB-009: EMR Integration via CSV

### Ambiguous Text
> "CSV file upload utility to pre-fill case report forms from EMR data"
> "Six care programs have established an EMR integration."

### Why It's Ambiguous
- CSV format specification is not provided
- Whether the six existing EMR integrations will be migrated is unclear
- Validation rules for CSV data are not specified
- Error handling and partial upload behavior is not defined

### Assumed Interpretation
A standard CSV format will be defined (or exists in Appendix A). The six existing EMR integrations will transition to CSV upload in the MVP, with real-time integration as a future opportunity.

### Alternative Interpretations
1. Existing EMR integrations must be preserved in their current form
2. Each care program may have different CSV formats
3. CSV upload is in addition to maintaining existing integrations

### Recommended Clarification
**Q1:** Is there an existing CSV specification for EMR data upload?
**Q2:** What happens to the six existing EMR integrations during the transition?
**Q3:** What EMR systems are in use at care centers (Epic, Cerner, etc.)?
**Q4:** Is there a standard data mapping between EMR fields and eCRF fields?

---

## AMB-010: Consent Management Scope

### Ambiguous Text
> "Streamlined and transparent consent tracking capturing variations in patient informed consent (e.g. collecting social security number, mental health outcomes, utilization as control arm for clinical trials) and applying appropriate downstream effects (e.g. field availability in eCRFs)"

### Why It's Ambiguous
This is listed as a "Future Opportunity" but consent is fundamental to registry operations. It's unclear:
- What consent management exists in current PortCF
- Whether basic consent tracking is in MVP scope
- How consent variations affect current eCRF field availability

### Assumed Interpretation
Basic consent tracking (enrolled/not enrolled) is part of MVP. Advanced consent variations and downstream field availability effects are future scope.

### Alternative Interpretations
1. Full consent management is required for MVP
2. Consent is entirely out of scope for MVP
3. Consent data will be migrated but not actively managed in NGR initially

### Recommended Clarification
**Q1:** What consent management functionality exists in current PortCF?
**Q2:** Is consent tracking required for MVP, and if so, at what level of granularity?
**Q3:** How do consent variations currently affect eCRF field availability?

---

## AMB-011: Report Builder Capabilities

### Ambiguous Text
> "Dynamic report building tool to produce patient lists"

### Why It's Ambiguous
- Query complexity limits are not specified
- Available fields for filtering are not defined
- Whether saved queries can be shared is unclear
- Export format options are not specified

### Assumed Interpretation
The report builder allows users to create ad-hoc queries against patient data fields, save queries for reuse, and export results. Functionality mirrors current PortCF capabilities.

### Alternative Interpretations
1. Report builder is a simple filter interface
2. Report builder supports complex SQL-like queries
3. Report builder includes visualization capabilities

### Recommended Clarification
**Q1:** What are the current PortCF report builder capabilities?
**Q2:** What fields should be available for querying?
**Q3:** Are there query complexity or result size limits?
**Q4:** Should saved queries be shareable between users?

---

## AMB-012: Training Environment Data

### Ambiguous Text
> "To successfully launch hands-on training, the training environment will need to be live (a copy of production)"

### Why It's Ambiguous
- Whether training uses real PHI or synthetic data is unclear
- How often the training environment is refreshed is not specified
- Whether care program users have individual training accounts is unclear

### Assumed Interpretation
Training environment uses a sanitized/de-identified copy of production data structure with synthetic patient data. It is refreshed periodically and provides isolated accounts for training purposes.

### Alternative Interpretations
1. Training uses actual production data (PHI concerns)
2. Training uses completely synthetic data
3. Each care program has its own training data subset

### Recommended Clarification
**Q1:** Should the training environment contain real PHI or synthetic data?
**Q2:** How often should the training environment be refreshed?
**Q3:** How are training user accounts provisioned for care program staff?

---

## AMB-013: Bug Severity Definitions Application

### Ambiguous Text
Bug severity definitions are provided, but application is ambiguous:
> "Critical: Significant business impact or a complete failure..."
> "High: Any feature failure..."
> "Low: Cosmetic or trivial issues..."

### Why It's Ambiguous
- Who determines severity classification (vendor, CFF, joint)?
- Dispute resolution for severity disagreements is not defined
- Whether severity can be reclassified is unclear

### Assumed Interpretation
Severity is initially assigned by the discovering party and confirmed jointly. CFF has final authority on severity classification for exit criteria purposes.

### Alternative Interpretations
1. Vendor assigns severity
2. CFF assigns severity
3. Severity is determined by objective criteria only

### Recommended Clarification
**Q1:** Who has authority to assign and confirm bug severity?
**Q2:** What is the process for disputing severity classification?
**Q3:** Can severity be reclassified during the resolution process?

---

## AMB-014: Co-Development Model Details

### Ambiguous Text
> "The Foundation expects a co-development approach with Foundation technical staff."

### Why It's Ambiguous
- Extent of CFF code contribution is unclear
- Code ownership and IP rights are not specified
- Whether CFF developers have commit access is unclear
- Quality standards for CFF-contributed code are not defined

### Assumed Interpretation
CFF technical staff will work alongside vendor developers, contributing to code review, testing, and potentially some development. Vendor maintains primary development responsibility. Code is owned by CFF.

### Alternative Interpretations
1. CFF developers write significant portions of code
2. Co-development is limited to requirements and testing
3. CFF developers shadow vendor developers for knowledge transfer only

### Recommended Clarification
**Q1:** What percentage of development work is expected from CFF technical staff?
**Q2:** Will CFF developers have direct commit access to the codebase?
**Q3:** What are the IP ownership terms for the developed solution?
**Q4:** How will code quality be ensured for contributions from both teams?

---

## AMB-015: Third-Party Security Certification Timeline

### Ambiguous Text
> "Assume that the Foundation will seek formal third-party validation of security and compliance, such as SOC 2 Type 2, HITRUST, or ISO 27001."

### Why It's Ambiguous
- Whether certification must be achieved before launch is unclear
- Which specific certification is required is not specified
- Whether vendor's existing certifications are acceptable is unclear
- Certification cost responsibility is not defined

### Assumed Interpretation
Security certification is a post-launch activity. The system must be designed and built to certification standards, with formal certification pursued after launch. CFF will select the specific certification.

### Alternative Interpretations
1. Certification required before launch
2. Vendor's existing certifications satisfy the requirement
3. Certification is CFF's responsibility post-handoff

### Recommended Clarification
**Q1:** Must security certification be achieved before launch?
**Q2:** Which certification does CFF prefer (SOC 2 Type 2, HITRUST, or ISO 27001)?
**Q3:** Can vendor's existing certifications be leveraged?
**Q4:** Who bears the cost of certification audits?

---

## AMB-016: Content Management Capabilities

### Ambiguous Text
> "Content management functionality allowing the Foundation to share and edit documentation and announcements with Care Centers via the graphical interface."

### Why It's Ambiguous
- Rich text editing capabilities are not specified
- Media support (images, videos, PDFs) is unclear
- Version history requirements are not defined
- Workflow/approval process for content is not specified

### Assumed Interpretation
Basic CMS functionality with rich text editing, document upload capability, and simple publish/unpublish workflow. No complex approval workflows required.

### Alternative Interpretations
1. Full CMS with media library and workflows
2. Simple text-only announcements
3. Integration with external CMS

### Recommended Clarification
**Q1:** What content types need to be supported (text, images, PDFs, videos)?
**Q2:** Is content versioning and history required?
**Q3:** Is an approval workflow needed before content publication?
**Q4:** Are there branding/template requirements for content?

---

## Summary

| ID | Category | Severity | Blocking |
|----|----------|----------|----------|
| AMB-001 | Documentation | High | Yes |
| AMB-002 | Technical | High | Yes |
| AMB-003 | Requirements | High | Yes |
| AMB-004 | Performance | Medium | No |
| AMB-005 | Integration | Medium | No |
| AMB-006 | Infrastructure | High | Yes |
| AMB-007 | Project | Medium | No |
| AMB-008 | Security | Medium | No |
| AMB-009 | Integration | Medium | No |
| AMB-010 | Functional | Medium | No |
| AMB-011 | Functional | Low | No |
| AMB-012 | Operational | Low | No |
| AMB-013 | Process | Low | No |
| AMB-014 | Process | Medium | No |
| AMB-015 | Compliance | Medium | No |
| AMB-016 | Functional | Low | No |

### Priority Clarifications Needed

**Before Architecture Design:**
1. AMB-001: Appendix contents (especially A and E)
2. AMB-002: SSO provider specification
3. AMB-006: Hosting and infrastructure preferences

**Before Development Planning:**
4. AMB-003: Form specification completeness
5. AMB-004: Performance specifications
6. AMB-005: Data warehouse integration details

**Before Detailed Design:**
7. AMB-008: User role definitions
8. AMB-009: EMR integration/CSV details
9. AMB-014: Co-development model details

---

## Recommendations

1. **Request Appendices Immediately:** The appendices contain critical requirements that significantly impact architecture, effort estimation, and solution design.

2. **Schedule Technical Discovery Session:** A working session with CFF technical team to clarify infrastructure, SSO, and integration questions.

3. **Request Current System Documentation:** PortCF technical documentation, data models, and user guides would accelerate understanding.

4. **Clarify Decision Authority:** Establish clear RACI for ambiguity resolution during the project.

5. **Document Assumptions:** All assumptions made in this analysis should be validated with CFF before finalizing architecture and estimates.
