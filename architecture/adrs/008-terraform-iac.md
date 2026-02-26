# ADR-008: Terraform for Infrastructure as Code

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires infrastructure provisioning for:
- 5 environments (dev, test, stage, train, prod)
- Consistent configuration across environments
- Auditable infrastructure changes
- Disaster recovery and environment recreation
- Compliance documentation for security audits

Requirements include:
- Reproducible deployments
- Version-controlled infrastructure
- Environment parity
- Separation of concerns (infra vs. application)

The RFP tech stack mandates Terraform with hashicorp/azurerm provider.

## Decision

We will use **Terraform** with the following structure:

### Provider Configuration
```hcl
terraform {
  required_version = ">= 1.6.0"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.85.0"
    }
  }
  
  backend "azurerm" {
    resource_group_name  = "rg-ngr-tfstate"
    storage_account_name = "stngrtfstate"
    container_name       = "tfstate"
    key                  = "ngr.tfstate"
  }
}
```

### Module Structure
```
infrastructure/
├── modules/
│   ├── app-service/
│   ├── sql-database/
│   ├── key-vault/
│   ├── application-gateway/
│   ├── storage/
│   └── monitoring/
├── environments/
│   ├── dev/
│   ├── test/
│   ├── stage/
│   ├── train/
│   └── prod/
└── shared/
    ├── variables.tf
    └── outputs.tf
```

### State Management
- Remote state in Azure Storage with locking
- Separate state files per environment
- State encryption at rest

## Consequences

### Positive
- **Reproducibility**: Identical infrastructure across environments
- **Version Control**: All changes tracked in Git
- **Drift Detection**: Terraform plan shows configuration drift
- **Documentation**: Infrastructure is self-documenting
- **Disaster Recovery**: Can recreate environments from code
- **Compliance**: Audit trail for all infrastructure changes

### Negative
- **Learning Curve**: HCL syntax requires training
- **State Management**: State file corruption can be catastrophic
- **Provider Lag**: New Azure features may not be immediately available
- **Complexity**: Module design requires careful planning

### Mitigations
- Use remote state with locking to prevent corruption
- Implement state backup procedures
- Create reusable modules for common patterns
- Document module usage and variables

## Alternatives Considered

### Azure Resource Manager (ARM) Templates
- **Rejected**: Not in approved tech stack
- JSON syntax is verbose and hard to maintain
- Limited modularity compared to Terraform

### Pulumi
- **Rejected**: Not in approved tech stack
- Would allow using C# for infrastructure
- Smaller community and ecosystem

### Azure Bicep
- **Rejected**: Not in approved tech stack
- Azure-specific, no multi-cloud support
- Newer with less enterprise adoption
