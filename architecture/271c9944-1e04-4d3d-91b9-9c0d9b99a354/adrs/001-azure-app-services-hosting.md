# ADR-001: Azure App Services for Application Hosting

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The Next Generation Patient Registry (NGR) requires a hosting platform for the ASP.NET Core 8 Web API and React 18 SPA. The platform must support:

- HIPAA-compliant PHI handling
- High availability (99.9%+ uptime)
- Auto-scaling for variable workloads
- Integration with Azure services (SQL, Key Vault, Application Insights)
- Cost-effective operations for a healthcare non-profit
- Simplified operations without dedicated DevOps staff

The RFP tech stack mandates Azure App Services (NOT AKS/Kubernetes).

## Decision

We will use **Azure App Services** for hosting both the API and web application:

1. **API Hosting**: Azure App Service (Linux, P2v3 tier)
   - ASP.NET Core 8 runtime
   - Deployment slots for zero-downtime deployments
   - Auto-scaling based on CPU/memory metrics

2. **Web App Hosting**: Azure Static Web Apps (Standard tier)
   - React 18 SPA with global CDN distribution
   - Integrated staging environments
   - Custom domain with managed SSL

## Consequences

### Positive
- **Simplified Operations**: PaaS model eliminates infrastructure management overhead
- **Built-in HA**: Multi-instance deployment with automatic load balancing
- **Native Azure Integration**: Seamless connection to Azure SQL, Key Vault, App Insights
- **Cost Predictable**: Fixed pricing tiers vs. variable container costs
- **Compliance Ready**: Azure App Service is HIPAA BAA eligible
- **Deployment Slots**: Enable blue-green deployments without additional tooling

### Negative
- **Less Flexibility**: Cannot customize underlying infrastructure
- **Scaling Limits**: Maximum 30 instances per App Service Plan
- **Cold Start**: Potential latency on scale-out events (mitigated by Always On)
- **Vendor Lock-in**: Tightly coupled to Azure platform

### Mitigations
- Enable "Always On" to prevent cold starts
- Configure auto-scale rules with appropriate thresholds
- Use deployment slots for staging and production swaps
- Implement health checks for automatic instance replacement

## Alternatives Considered

### Azure Kubernetes Service (AKS)
- **Rejected**: Explicitly excluded by RFP tech stack requirements
- Would provide more flexibility but requires Kubernetes expertise
- Higher operational complexity for a small team

### Azure Container Apps
- **Rejected**: Not in approved tech stack
- Newer service with less enterprise adoption
- Would require containerization of applications

### Azure Virtual Machines
- **Rejected**: Too much operational overhead
- Requires manual patching, scaling, and load balancing
- Not cost-effective for this workload
