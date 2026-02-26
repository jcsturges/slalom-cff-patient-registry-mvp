# ADR-001: Cloud Platform Selection - Microsoft Azure

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The Next Generation Registry (NGR) requires a cloud platform to host all infrastructure components. The platform must support HIPAA compliance, provide managed services for databases and Kubernetes, and integrate with the Cystic Fibrosis Foundation's existing technology ecosystem.

The Foundation currently uses:
- Azure Active Directory for identity management
- Azure DevOps for project management and CI/CD
- Microsoft Teams for collaboration

## Decision

We will use **Microsoft Azure** as the cloud platform for all NGR infrastructure.

## Consequences

### Positive
- Native integration with Azure AD for SSO eliminates additional identity bridging
- Azure DevOps pipelines can deploy directly to Azure resources with managed identities
- HIPAA BAA available at platform level
- Foundation team familiarity reduces learning curve
- Comprehensive managed services (AKS, PostgreSQL Flexible Server, Redis Cache)
- Strong compliance certifications (SOC 2, HITRUST, ISO 27001)
- ExpressRoute/VPN options for secure connectivity to Foundation data warehouse

### Negative
- Vendor lock-in to Microsoft ecosystem
- Some Azure services have less mature tooling compared to AWS equivalents
- Cost optimization requires Azure-specific expertise

### Risks
- Azure service outages affect entire system (mitigated by multi-AZ deployment)
- Pricing changes could impact operational costs

## Alternatives Considered

### Amazon Web Services (AWS)
- **Pros:** Most mature cloud platform, extensive service catalog, strong healthcare presence
- **Cons:** Would require identity federation with Azure AD, no existing Foundation relationship, team would need AWS training
- **Rejected because:** Integration complexity with existing Azure AD and DevOps would add project risk and timeline

### Google Cloud Platform (GCP)
- **Pros:** Strong Kubernetes heritage (GKE), competitive pricing, good data analytics
- **Cons:** Smallest healthcare market presence, would require identity federation, limited Foundation familiarity
- **Rejected because:** Least alignment with Foundation's existing technology investments

### On-Premises / Hybrid
- **Pros:** Maximum control, no cloud vendor dependency
- **Cons:** Significant capital expenditure, operational burden, scaling limitations, compliance complexity
- **Rejected because:** Does not align with modern healthcare IT practices, higher total cost of ownership
