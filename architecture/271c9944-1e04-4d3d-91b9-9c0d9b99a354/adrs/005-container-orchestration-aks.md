# ADR-005: Container Orchestration - Azure Kubernetes Service (AKS)

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system consists of multiple backend services that need to be deployed, scaled, and managed. The orchestration platform must support:
- Horizontal auto-scaling based on load
- Rolling deployments with zero downtime
- Service discovery and load balancing
- Secret management integration
- Health monitoring and self-healing
- Environment isolation (dev, test, stage, prod)

## Decision

We will use **Azure Kubernetes Service (AKS)** for container orchestration with **Helm 3.x** for package management.

## Consequences

### Positive
- Industry-standard orchestration with strong Azure integration
- Managed control plane reduces operational burden
- Native integration with Azure AD for RBAC
- Azure Monitor and Container Insights for observability
- Horizontal Pod Autoscaler (HPA) for automatic scaling
- Network policies for micro-segmentation
- Azure Key Vault integration via CSI driver
- Multi-AZ deployment for high availability
- Cluster autoscaler for node-level scaling

### Negative
- Kubernetes complexity requires specialized knowledge
- Higher baseline cost than simpler alternatives
- Networking complexity (CNI, ingress, service mesh considerations)

### Risks
- Kubernetes version upgrades require planning and testing
- Misconfiguration can lead to security vulnerabilities
- Resource over-provisioning if not properly tuned

## Alternatives Considered

### Azure App Service
- **Pros:** Simpler deployment model, managed scaling, lower operational overhead
- **Cons:** Less flexibility, limited networking options, harder to maintain service isolation
- **Rejected because:** Multi-service architecture benefits from Kubernetes' service mesh capabilities

### Azure Container Apps
- **Pros:** Serverless containers, simpler than AKS, Dapr integration
- **Cons:** Less mature, fewer networking options, limited customization
- **Rejected because:** Enterprise healthcare requirements need AKS's maturity and control

### Azure Container Instances (ACI)
- **Pros:** Simple container deployment, pay-per-second, quick startup
- **Cons:** No orchestration, manual scaling, limited networking
- **Rejected because:** Does not provide orchestration features needed for multi-service system

### Docker Swarm
- **Pros:** Simpler than Kubernetes, Docker native
- **Cons:** Limited ecosystem, declining adoption, fewer managed offerings
- **Rejected because:** Kubernetes is industry standard with better long-term support

### Amazon EKS
- **Pros:** Mature Kubernetes offering, strong ecosystem
- **Cons:** AWS-only, conflicts with Azure platform decision
- **Rejected because:** Azure alignment requirement from ADR-001
