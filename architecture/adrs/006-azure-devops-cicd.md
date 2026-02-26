# ADR-006: Azure DevOps for CI/CD Pipelines

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires a CI/CD platform to:
- Build and test code on every commit
- Enforce quality gates (code coverage, security scans)
- Deploy to multiple environments (dev, test, stage, train, prod)
- Support approval workflows for production deployments
- Integrate with Azure services for deployment
- Track work items and link to code changes

Requirements from PRD:
- Automated tests integrated into build/deployment pipeline
- Unit tests with minimum 70% code coverage
- SonarQube integration for code quality
- OWASP ZAP for security scanning
- Foundation's Azure DevOps instance will be used

The RFP tech stack mandates Azure DevOps YAML Pipelines.

## Decision

We will use **Azure DevOps** with YAML-based pipelines:

### Pipeline Structure
1. **Build Pipeline** (CI)
   - Restore dependencies
   - Build solution
   - Run unit tests with coverage
   - SonarQube analysis
   - OWASP ZAP scan
   - Publish artifacts

2. **Release Pipeline** (CD)
   - Deploy to Dev (automatic)
   - Deploy to Test (automatic after Dev)
   - Deploy to Stage (manual approval)
   - Deploy to Prod (manual approval + change window)

### Quality Gates
- Minimum 70% code coverage
- No critical SonarQube issues
- No high/critical OWASP vulnerabilities
- All unit tests passing

### Branch Strategy
- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - Feature branches
- `release/*` - Release candidates
- `hotfix/*` - Production fixes

## Consequences

### Positive
- **Native Azure Integration**: Seamless deployment to Azure services
- **YAML Pipelines**: Version-controlled, reviewable pipeline definitions
- **Built-in Approvals**: Native support for deployment gates
- **Work Item Linking**: Traceability from requirements to deployments
- **Artifact Management**: Built-in artifact storage and versioning
- **Foundation Familiarity**: Already used by CF Foundation

### Negative
- **Learning Curve**: YAML syntax can be complex
- **Limited Parallelism**: Free tier has limited parallel jobs
- **Microsoft Lock-in**: Tightly coupled to Azure ecosystem
- **UI Complexity**: Azure DevOps UI can be overwhelming

### Mitigations
- Create reusable YAML templates for common tasks
- Document pipeline patterns and troubleshooting guides
- Use pipeline caching to reduce build times
- Implement clear naming conventions for pipelines

## Alternatives Considered

### GitHub Actions
- **Rejected**: Foundation uses Azure DevOps for work tracking
- Would require separate tool for work items
- Less native Azure integration

### Jenkins
- **Rejected**: Requires self-hosted infrastructure
- Higher operational overhead
- Less Azure-native integration

### GitLab CI/CD
- **Rejected**: Would require migration from Azure DevOps
- Different work item tracking system
- Additional licensing costs
