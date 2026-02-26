# ADR-010: Infrastructure as Code - Terraform

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires infrastructure provisioning for:
- Multiple environments (dev, test, stage, training, production)
- Azure resources (AKS, PostgreSQL, Redis, Storage, Networking)
- Consistent, repeatable deployments
- Audit trail for infrastructure changes
- Disaster recovery and environment recreation

## Decision

We will use **Terraform 1.6.x** with **Azure Provider** for all infrastructure provisioning, storing state in **Azure Blob Storage** with state locking via **Azure Storage lease**.

### Repository Structure
```
infrastructure/
├── modules/
│   ├── aks/
│   ├── postgresql/
│   ├── redis/
│   ├── networking/
│   └── storage/
├── environments/
│   ├── dev/
│   ├── test/
│   ├── stage/
│   ├── training/
│   └── prod/
└── shared/
    └── state-storage/
```

## Consequences

### Positive
- Declarative infrastructure definition
- State management enables drift detection
- Module reuse across environments
- Plan/apply workflow provides change preview
- Large community and extensive Azure provider
- Version control for infrastructure changes
- Supports multi-cloud if ever needed
- Terraform Cloud/Enterprise available for enhanced features

### Negative
- State file management complexity
- Learning curve for team members new to Terraform
- Provider version updates can introduce breaking changes
- State locking issues in concurrent operations

### Risks
- State file corruption could impact deployments (mitigated by state backups)
- Terraform provider bugs could affect Azure resources
- Secret management requires careful handling (use Azure Key Vault references)

## Alternatives Considered

### Azure Resource Manager (ARM) Templates
- **Pros:** Native Azure, no additional tooling, Azure-supported
- **Cons:** Verbose JSON, limited modularity, Azure-only, harder to read
- **Rejected because:** Terraform provides better developer experience and modularity

### Azure Bicep
- **Pros:** Native Azure, cleaner syntax than ARM, Microsoft-supported
- **Cons:** Azure-only, smaller community than Terraform, less mature tooling
- **Rejected because:** Terraform's multi-cloud capability and larger ecosystem preferred

### Pulumi
- **Pros:** Real programming languages, strong typing, modern approach
- **Cons:** Smaller community, less mature, requires programming knowledge for infra team
- **Rejected because:** Terraform's declarative approach and larger community preferred

### AWS CloudFormation / CDK
- **Pros:** Native AWS, good tooling
- **Cons:** AWS-only, conflicts with Azure platform decision
- **Rejected because:** Azure alignment requirement from ADR-001

### Ansible
- **Pros:** Agentless, procedural and declarative, good for configuration
- **Cons:** Not designed for cloud infrastructure, state management limitations
- **Rejected because:** Terraform better suited for cloud resource provisioning
