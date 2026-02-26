# ADR-005: Hosting Platform Selection

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires a hosting platform that supports:

- React SPA frontend
- ASP.NET Core 8 Web API backend
- High availability with load balancing
- Auto-scaling for variable workloads
- Integration with Azure services (SQL, Key Vault, Blob Storage)
- CI/CD deployment from Azure DevOps
- HIPAA compliance
- Cost-effective for expected user load (~3,000 users, ~300 concurrent)

## Decision

We will use **Azure App Service** (Premium v3 tier) for hosting both the frontend and backend applications.

**Configuration:**

| Component | App Service Plan | SKU | Instances |
|-----------|------------------|-----|-----------|
| Web App (React) | asp-ngr-web-prod | P1v3 | 2-4 |
| API (ASP.NET Core) | asp-ngr-api-prod | P2v3 | 2-6 |

**Features enabled:**
- Always On: Enabled (prevents cold starts)
- Auto-scale: CPU and memory-based rules
- Deployment slots: Staging slot for blue/green deployments
- VNet integration: Private endpoint access to Azure SQL
- Managed Identity: For Azure resource access (Key Vault, Blob Storage)

**Network architecture:**
- Azure Front Door: Global load balancing, SSL termination, CDN
- Azure WAF: OWASP 3.2 ruleset, custom rules
- Private endpoints: Azure SQL accessible only via VNet

## Consequences

### Positive
- **Mandated by tech stack**: Aligns with Foundation's required technology stack
- **Managed platform**: No infrastructure management overhead
- **Auto-scaling**: Handles variable workloads automatically
- **High availability**: Built-in load balancing and health checks
- **Deployment slots**: Zero-downtime deployments
- **Azure integration**: Native integration with Azure services
- **Compliance**: HIPAA BAA available, SOC 2 certified
- **Cost predictable**: Fixed pricing per App Service Plan

### Negative
- **Less control**: Limited OS-level customization compared to VMs
- **Cold starts**: Possible on lower tiers (mitigated by Always On)
- **Scaling limits**: Maximum 30 instances per plan
- **Egress costs**: Data transfer costs for high-volume scenarios

### Risks
- **Regional outage**: Single-region deployment; DR requires secondary region
- **Resource contention**: Shared infrastructure in multi-tenant environment
- **Scaling delays**: Auto-scale takes 5-10 minutes to respond

## Alternatives Considered

### Azure Kubernetes Service (AKS)
- **Rejected**: Not in mandated tech stack; higher operational complexity; overkill for current scale

### Azure Container Apps
- **Rejected**: Not in mandated tech stack; less mature than App Service

### Azure Virtual Machines
- **Rejected**: Higher operational overhead; requires manual scaling and patching
