# ADR-006: Container Orchestration - Azure Kubernetes Service

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires a container orchestration platform to deploy and manage microservices. The platform must:
- Support horizontal scaling for variable load
- Provide high availability across availability zones
- Enable zero-downtime deployments
- Support multiple environments (dev, test, stage, prod, training)
- Integrate with CI/CD pipelines
- Provide resource isolation between services

## Decision

We will use **Azure Kubernetes Service (AKS)** version 1.28 for container orchestration.

Configuration:
- Separate AKS clusters per environment (prod, non-prod)
- System and application node pools
- Cluster autoscaler enabled
- Azure CNI networking with Calico network policies
- Azure AD integration for cluster access
- Managed identity for Azure resource access

## Consequences

### Positive
- **Industry Standard**: Kubernetes is the de facto standard for container orchestration
- **Portability**: Workloads can be migrated to other Kubernetes platforms if needed
- **Scaling**: Native horizontal pod autoscaling and cluster autoscaling
- **Ecosystem**: Rich ecosystem of tools (Helm, Kustomize, operators)
- **Azure Integration**: Native integration with Azure AD, Key Vault, Monitor
- **Managed Control Plane**: Azure manages Kubernetes control plane, reducing operational burden
- **Multi-tenancy**: Namespaces provide logical isolation between environments/services

### Negative
- **Complexity**: Kubernetes has a steep learning curve
- **Cost**: AKS has costs for node VMs and some features
- **Operational Overhead**: Still requires cluster management, upgrades, monitoring
- **Resource Overhead**: Kubernetes system components consume cluster resources

### Mitigations
- Use Helm charts for standardized deployments
- Implement GitOps with Azure DevOps for declarative configuration
- Establish runbooks for common operational tasks
- Use Azure Monitor and Container Insights for observability
- Schedule regular cluster upgrades during maintenance windows

## Alternatives Considered

### Azure Container Apps
- **Pros**: Simpler than Kubernetes, serverless scaling, lower operational overhead
- **Cons**: Less control, newer service, limited customization
- **Rejected**: Need more control for complex healthcare application requirements

### Azure App Service
- **Pros**: Simple PaaS deployment, managed scaling
- **Cons**: Less flexibility, container support limited, harder to maintain consistency across services
- **Rejected**: Microservices architecture better suited to Kubernetes

### Docker Swarm
- **Pros**: Simpler than Kubernetes, Docker-native
- **Cons**: Limited ecosystem, less industry adoption, fewer managed offerings
- **Rejected**: Kubernetes ecosystem and Azure managed offering preferred

### Amazon ECS/EKS
- **Pros**: Mature container services, good AWS integration
- **Cons**: Would require AWS instead of Azure, no existing Foundation relationship
- **Rejected**: Azure selected as cloud platform (see ADR-001)
