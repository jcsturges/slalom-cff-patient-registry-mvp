# System Design Document
## Next Generation Patient Registry (NGR) - Cystic Fibrosis Foundation

**Document Version:** 1.0  
**Date:** 2024-01-15  
**Project:** Next Generation Patient Registry (NGR)  
**Document ID:** SDD-NGR-001

---

## 1. Architecture Overview

### 1.1 C4 Model - System Context Diagram (Level 1)

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              EXTERNAL ACTORS                                      │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                   │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐           │
│  │   Care Program   │    │   Foundation     │    │   System         │           │
│  │   Users (3000+)  │    │   Staff          │    │   Administrators │           │
│  │   - Physicians   │    │   - Analysts     │    │                  │           │
│  │   - Nurses       │    │   - Researchers  │    │                  │           │
│  │   - Clinicians   │    │   - Admins       │    │                  │           │
│  └────────┬─────────┘    └────────┬─────────┘    └────────┬─────────┘           │
│           │                       │                       │                      │
│           └───────────────────────┼───────────────────────┘                      │
│                                   │                                              │
│                                   ▼                                              │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                    NEXT GENERATION REGISTRY (NGR)                        │    │
│  │                                                                          │    │
│  │   Patient Registry Platform for Cystic Fibrosis Foundation              │    │
│  │   - Patient Roster Management                                            │    │
│  │   - Electronic Case Report Forms (eCRFs)                                 │    │
│  │   - Dynamic Reporting                                                    │    │
│  │   - Content Management                                                   │    │
│  │   - Data Import/Export                                                   │    │
│  │                                                                          │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                   │                                              │
│           ┌───────────────────────┼───────────────────────┐                      │
│           │                       │                       │                      │
│           ▼                       ▼                       ▼                      │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐           │
│  │   Okta           │    │   Foundation     │    │   Azure          │           │
│  │   Identity       │    │   Data           │    │   Services       │           │
│  │   Provider       │    │   Warehouse      │    │   (Monitoring)   │           │
│  │   (OIDC/SAML)    │    │                  │    │                  │           │
│  └──────────────────┘    └──────────────────┘    └──────────────────┘           │
│                                                                                   │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 C4 Model - Container Diagram (Level 2)

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              NGR SYSTEM BOUNDARY                                          │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           AZURE APP SERVICES                                      │   │
│   │                                                                                   │   │
│   │   ┌─────────────────────────┐         ┌─────────────────────────┐               │   │
│   │   │     NGR Web App         │         │     NGR API             │               │   │
│   │   │     (React 18 SPA)      │◄───────►│     (ASP.NET Core 8)    │               │   │
│   │   │                         │  HTTPS  │                         │               │   │
│   │   │   - Patient Roster UI   │         │   - REST API            │               │   │
│   │   │   - eCRF Forms          │         │   - Business Logic      │               │   │
│   │   │   - Report Builder      │         │   - Data Access Layer   │               │   │
│   │   │   - Content Viewer      │         │   - Validation Engine   │               │   │
│   │   │   - Admin Console       │         │   - File Processing     │               │   │
│   │   │                         │         │                         │               │   │
│   │   └───────────┬─────────────┘         └───────────┬─────────────┘               │   │
│   │               │                                   │                              │   │
│   └───────────────┼───────────────────────────────────┼──────────────────────────────┘   │
│                   │                                   │                                   │
│                   │                                   │                                   │
│   ┌───────────────▼───────────────┐   ┌──────────────▼──────────────────┐               │
│   │       Azure WAF               │   │       Azure SQL Database        │               │
│   │   (Web Application Firewall)  │   │                                 │               │
│   │                               │   │   - Patient Data                │               │
│   │   - OWASP Protection          │   │   - Encounter Records           │               │
│   │   - DDoS Mitigation           │   │   - Form Definitions            │               │
│   │   - Rate Limiting             │   │   - User/Role Data              │               │
│   │                               │   │   - Audit Logs                  │               │
│   └───────────────────────────────┘   │   - Content/Announcements       │               │
│                                       │                                 │               │
│                                       └─────────────────────────────────┘               │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           SUPPORTING SERVICES                                     │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐               │   │
│   │   │  Azure Key      │   │  Azure Blob     │   │  Azure App      │               │   │
│   │   │  Vault          │   │  Storage        │   │  Insights       │               │   │
│   │   │                 │   │                 │   │                 │               │   │
│   │   │  - Secrets      │   │  - CSV Uploads  │   │  - Telemetry    │               │   │
│   │   │  - Certificates │   │  - Documents    │   │  - Logging      │               │   │
│   │   │  - Keys         │   │  - Exports      │   │  - Metrics      │               │   │
│   │   └─────────────────┘   └─────────────────┘   └─────────────────┘               │   │
│   │                                                                                   │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
└─────────────────────────────────────────────────────────────────────────────────────────┘
                    │                                           │
                    │                                           │
                    ▼                                           ▼
        ┌───────────────────────┐                   ┌───────────────────────┐
        │       Okta            │                   │   Foundation Data     │
        │   Identity Provider   │                   │   Warehouse           │
        │                       │                   │                       │
        │   - OIDC Auth         │                   │   - Nightly Feed      │
        │   - SAML SSO          │                   │   - Bi-directional    │
        │   - MFA               │                   │   - Full/Incremental  │
        └───────────────────────┘                   └───────────────────────┘
```

### 1.3 C4 Model - Component Diagram (Level 3) - API Service

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              NGR API (ASP.NET Core 8)                                    │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           PRESENTATION LAYER                                      │   │
│   │                                                                                   │   │
│   │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │   │
│   │   │  Patients   │  │   Forms     │  │  Reports    │  │  Content    │            │   │
│   │   │  Controller │  │  Controller │  │  Controller │  │  Controller │            │   │
│   │   └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘            │   │
│   │          │                │                │                │                    │   │
│   │   ┌──────┴────────────────┴────────────────┴────────────────┴──────┐            │   │
│   │   │                    API Middleware Pipeline                      │            │   │
│   │   │  - Authentication (Okta JWT)  - Authorization (RBAC)           │            │   │
│   │   │  - Exception Handling         - Request Logging                 │            │   │
│   │   │  - Rate Limiting              - CORS                            │            │   │
│   │   └─────────────────────────────────────────────────────────────────┘            │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           APPLICATION LAYER                                       │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                 │   │
│   │   │  Patient        │  │  Form           │  │  Report         │                 │   │
│   │   │  Service        │  │  Service        │  │  Service        │                 │   │
│   │   │                 │  │                 │  │                 │                 │   │
│   │   │  - Roster Mgmt  │  │  - eCRF Logic   │  │  - Query Builder│                 │   │
│   │   │  - Transfer     │  │  - Validation   │  │  - Export       │                 │   │
│   │   │  - Merge        │  │  - Versioning   │  │  - Standard Rpts│                 │   │
│   │   └─────────────────┘  └─────────────────┘  └─────────────────┘                 │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                 │   │
│   │   │  Content        │  │  Import         │  │  DataWarehouse  │                 │   │
│   │   │  Service        │  │  Service        │  │  Service        │                 │   │
│   │   │                 │  │                 │  │                 │                 │   │
│   │   │  - Documents    │  │  - CSV Parse    │  │  - Feed Gen     │                 │   │
│   │   │  - Announcements│  │  - Field Map    │  │  - Sync         │                 │   │
│   │   │  - CMS          │  │  - Pre-fill     │  │  - Reconcile    │                 │   │
│   │   └─────────────────┘  └─────────────────┘  └─────────────────┘                 │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           DOMAIN LAYER                                            │   │
│   │                                                                                   │   │
│   │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │   │
│   │   │  Patient    │  │  Encounter  │  │  Form       │  │  CareProgram│            │   │
│   │   │  Entity     │  │  Entity     │  │  Entity     │  │  Entity     │            │   │
│   │   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘            │   │
│   │                                                                                   │   │
│   │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │   │
│   │   │  User       │  │  Role       │  │  AuditLog   │  │  Content    │            │   │
│   │   │  Entity     │  │  Entity     │  │  Entity     │  │  Entity     │            │   │
│   │   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘            │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           INFRASTRUCTURE LAYER                                    │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                 │   │
│   │   │  EF Core        │  │  Okta           │  │  Azure Blob     │                 │   │
│   │   │  DbContext      │  │  Integration    │  │  Storage Client │                 │   │
│   │   └─────────────────┘  └─────────────────┘  └─────────────────┘                 │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                 │   │
│   │   │  Key Vault      │  │  App Insights   │  │  Data Warehouse │                 │   │
│   │   │  Client         │  │  Telemetry      │  │  Connector      │                 │   │
│   │   └─────────────────┘  └─────────────────┘  └─────────────────┘                 │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Component Breakdown

### 2.1 NGR Web Application (React 18 SPA)

| Aspect | Details |
|--------|---------|
| **Responsibility** | Single-page application providing user interface for all NGR functionality |
| **Technology** | React 18.2.0, TypeScript 5.3, Vite 5.0 |
| **Key Libraries** | @okta/okta-react 6.7.0, react-router-dom 6.21.0, @tanstack/react-query 5.17.0, react-hook-form 7.49.0, zod 3.22.4, @mui/material 5.15.0 |
| **Hosting** | Azure App Service (Linux, Node 20 LTS) |
| **State Management** | React Query for server state, React Context for auth state |

**Modules:**
- **Authentication Module**: Okta integration, token management, session handling
- **Patient Roster Module**: Patient list, search, add, transfer, merge operations
- **eCRF Module**: Dynamic form rendering, validation, submission
- **Reports Module**: Report builder UI, standard reports, export functionality
- **Content Module**: Document viewer, announcements display
- **Admin Module**: User management, role assignment, system configuration

### 2.2 NGR API (ASP.NET Core 8 Web API)

| Aspect | Details |
|--------|---------|
| **Responsibility** | RESTful API providing business logic, data access, and integrations |
| **Technology** | ASP.NET Core 8.0, C# 12, Entity Framework Core 8.0 |
| **Key Packages** | Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0, Okta.AspNetCore 4.5.0, Microsoft.EntityFrameworkCore.SqlServer 8.0.0, Azure.Identity 1.10.4, Azure.Security.KeyVault.Secrets 4.5.0, Azure.Storage.Blobs 12.19.1, Swashbuckle.AspNetCore 6.5.0 |
| **Hosting** | Azure App Service (Windows, .NET 8) |
| **Architecture** | Clean Architecture with CQRS pattern |

**Services:**
- **PatientService**: Patient CRUD, roster management, transfer, merge logic
- **FormService**: eCRF management, validation engine, form versioning
- **ReportService**: Dynamic query builder, standard reports, export generation
- **ContentService**: CMS operations, document management, announcements
- **ImportService**: CSV parsing, field mapping, pre-fill logic
- **DataWarehouseService**: Feed generation, synchronization, reconciliation
- **AuditService**: Comprehensive audit logging for all operations

### 2.3 Azure SQL Database

| Aspect | Details |
|--------|---------|
| **Responsibility** | Primary data store for all NGR data |
| **Technology** | Azure SQL Database (General Purpose, Gen5, 8 vCores) |
| **Features** | Transparent Data Encryption (TDE), Geo-redundant backup, Point-in-time restore |
| **Capacity** | 1 TB initial, auto-grow enabled |
| **Performance** | Read replicas for reporting queries |

---

## 3. Technology Stack

### 3.1 Frontend Stack

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| Framework | React | 18.2.0 | UI component library |
| Language | TypeScript | 5.3.3 | Type-safe JavaScript |
| Build Tool | Vite | 5.0.10 | Fast build and HMR |
| Routing | react-router-dom | 6.21.1 | Client-side routing |
| State Management | @tanstack/react-query | 5.17.1 | Server state management |
| Forms | react-hook-form | 7.49.2 | Form handling |
| Validation | zod | 3.22.4 | Schema validation |
| UI Components | @mui/material | 5.15.2 | Material Design components |
| Auth | @okta/okta-react | 6.7.0 | Okta React SDK |
| Auth | @okta/okta-auth-js | 7.5.1 | Okta Auth JS SDK |
| HTTP Client | axios | 1.6.3 | API communication |
| Testing | Playwright | 1.40.1 | E2E testing |
| Testing | Vitest | 1.1.1 | Unit testing |

### 3.2 Backend Stack

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| Framework | ASP.NET Core | 8.0.0 | Web API framework |
| Language | C# | 12.0 | Primary language |
| ORM | Entity Framework Core | 8.0.0 | Data access |
| Auth | Okta.AspNetCore | 4.5.0 | Okta integration |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT validation |
| Secrets | Azure.Security.KeyVault.Secrets | 4.5.0 | Secret management |
| Storage | Azure.Storage.Blobs | 12.19.1 | File storage |
| Monitoring | Microsoft.ApplicationInsights.AspNetCore | 2.22.0 | Telemetry |
| API Docs | Swashbuckle.AspNetCore | 6.5.0 | OpenAPI/Swagger |
| Validation | FluentValidation | 11.9.0 | Request validation |
| Mapping | AutoMapper | 12.0.1 | Object mapping |
| Testing | xUnit | 2.6.4 | Unit testing |
| Testing | Moq | 4.20.70 | Mocking |
| Testing | FluentAssertions | 6.12.0 | Assertion library |

### 3.3 Infrastructure Stack

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| IaC | Terraform | 1.6.x | Infrastructure as Code |
| Provider | hashicorp/azurerm | 3.85.0 | Azure provider |
| Hosting | Azure App Service | P2v3 | Application hosting |
| Database | Azure SQL Database | Gen5 | Data storage |
| WAF | Azure Web Application Firewall | 2.0 | Security |
| Monitoring | Azure Application Insights | - | Observability |
| Secrets | Azure Key Vault | - | Secret management |
| Storage | Azure Blob Storage | - | File storage |
| Identity | Okta | - | Authentication |

### 3.4 CI/CD Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| Pipeline | Azure DevOps YAML Pipelines | Build and deployment |
| Code Quality | SonarQube | Static analysis |
| Security Scan | OWASP ZAP | Dynamic security testing |
| Container Registry | Azure Container Registry | Docker images (future) |

---

## 4. Data Flow

### 4.1 User Authentication Flow

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  User    │     │  React   │     │  Okta    │     │  NGR API │     │  Azure   │
│  Browser │     │  App     │     │  IdP     │     │          │     │  SQL     │
└────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘
     │                │                │                │                │
     │ 1. Access App  │                │                │                │
     │───────────────►│                │                │                │
     │                │                │                │                │
     │                │ 2. Redirect to │                │                │
     │                │    Okta Login  │                │                │
     │◄───────────────│───────────────►│                │                │
     │                │                │                │                │
     │ 3. Enter       │                │                │                │
     │    Credentials │                │                │                │
     │───────────────────────────────►│                │                │
     │                │                │                │                │
     │                │ 4. Auth Code   │                │                │
     │◄───────────────────────────────│                │                │
     │                │                │                │                │
     │ 5. Exchange    │                │                │                │
     │    for Tokens  │                │                │                │
     │───────────────►│───────────────►│                │                │
     │                │                │                │                │
     │                │ 6. ID Token +  │                │                │
     │                │    Access Token│                │                │
     │                │◄───────────────│                │                │
     │                │                │                │                │
     │                │ 7. API Request │                │                │
     │                │    with Token  │                │                │
     │                │───────────────────────────────►│                │
     │                │                │                │                │
     │                │                │ 8. Validate    │                │
     │                │                │    Token       │                │
     │                │                │◄───────────────│                │
     │                │                │                │                │
     │                │                │                │ 9. Query Data  │
     │                │                │                │───────────────►│
     │                │                │                │                │
     │                │                │                │ 10. Return     │
     │                │                │                │◄───────────────│
     │                │                │                │                │
     │                │ 11. Response   │                │                │
     │◄───────────────│◄───────────────────────────────│                │
     │                │                │                │                │
```

### 4.2 Patient Data Entry Flow (eCRF Submission)

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  Care    │     │  React   │     │  NGR API │     │  Azure   │     │  App     │
│  User    │     │  App     │     │          │     │  SQL     │     │ Insights │
└────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘
     │                │                │                │                │
     │ 1. Select      │                │                │                │
     │    Patient     │                │                │                │
     │───────────────►│                │                │                │
     │                │                │                │                │
     │                │ 2. GET /patients/{id}/forms     │                │
     │                │───────────────►│                │                │
     │                │                │                │                │
     │                │                │ 3. Fetch Form  │                │
     │                │                │    Definitions │                │
     │                │                │───────────────►│                │
     │                │                │◄───────────────│                │
     │                │                │                │                │
     │                │ 4. Form Schema │                │                │
     │◄───────────────│◄───────────────│                │                │
     │                │                │                │                │
     │ 5. Fill Form   │                │                │                │
     │    Data        │                │                │                │
     │───────────────►│                │                │                │
     │                │                │                │                │
     │                │ 6. Client-side │                │                │
     │                │    Validation  │                │                │
     │                │                │                │                │
     │                │ 7. POST /forms │                │                │
     │                │───────────────►│                │                │
     │                │                │                │                │
     │                │                │ 8. Server      │                │
     │                │                │    Validation  │                │
     │                │                │                │                │
     │                │                │ 9. Save Form   │                │
     │                │                │───────────────►│                │
     │                │                │◄───────────────│                │
     │                │                │                │                │
     │                │                │ 10. Audit Log  │                │
     │                │                │───────────────►│                │
     │                │                │                │                │
     │                │                │ 11. Telemetry  │                │
     │                │                │───────────────────────────────►│
     │                │                │                │                │
     │                │ 12. Success    │                │                │
     │◄───────────────│◄───────────────│                │                │
     │                │                │                │                │
```

### 4.3 CSV Import Flow

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  Care    │     │  React   │     │  NGR API │     │  Blob    │     │  Azure   │
│  User    │     │  App     │     │          │     │  Storage │     │  SQL     │
└────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘
     │                │                │                │                │
     │ 1. Upload CSV  │                │                │                │
     │───────────────►│                │                │                │
     │                │                │                │                │
     │                │ 2. POST /imports/upload         │                │
     │                │───────────────►│                │                │
     │                │                │                │                │
     │                │                │ 3. Store CSV   │                │
     │                │                │───────────────►│                │
     │                │                │◄───────────────│                │
     │                │                │                │                │
     │                │                │ 4. Parse &     │                │
     │                │                │    Validate    │                │
     │                │                │                │                │
     │                │                │ 5. Map Fields  │                │
     │                │                │    to eCRF     │                │
     │                │                │                │                │
     │                │ 6. Preview     │                │                │
     │◄───────────────│◄───────────────│                │                │
     │                │                │                │                │
     │ 7. Review &    │                │                │                │
     │    Confirm     │                │                │                │
     │───────────────►│                │                │                │
     │                │                │                │                │
     │                │ 8. POST /imports/{id}/confirm   │                │
     │                │───────────────►│                │                │
     │                │                │                │                │
     │                │                │ 9. Create      │                │
     │                │                │    Pre-filled  │                │
     │                │                │    Forms       │                │
     │                │                │───────────────────────────────►│
     │                │                │◄───────────────────────────────│
     │                │                │                │                │
     │                │ 10. Success    │                │                │
     │◄───────────────│◄───────────────│                │                │
     │                │                │                │                │
```

### 4.4 Data Warehouse Feed Flow

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  Azure   │     │  NGR API │     │  Azure   │     │  Data    │
│  SQL     │     │  (Job)   │     │  Blob    │     │ Warehouse│
└────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘
     │                │                │                │
     │                │ 1. Scheduled   │                │
     │                │    Trigger     │                │
     │                │    (Nightly)   │                │
     │                │                │                │
     │ 2. Query       │                │                │
     │    Changed     │                │                │
     │    Records     │                │                │
     │◄───────────────│                │                │
     │───────────────►│                │                │
     │                │                │                │
     │                │ 3. Transform   │                │
     │                │    to DW       │                │
     │                │    Schema      │                │
     │                │                │                │
     │                │ 4. Generate    │                │
     │                │    Feed Files  │                │
     │                │───────────────►│                │
     │                │◄───────────────│                │
     │                │                │                │
     │                │ 5. Notify DW   │                │
     │                │    (or Push)   │                │
     │                │───────────────────────────────►│
     │                │                │                │
     │                │                │ 6. DW Pulls    │
     │                │                │    Feed Files  │
     │                │                │◄───────────────│
     │                │                │───────────────►│
     │                │                │                │
     │                │ 7. Reconcile   │                │
     │                │◄───────────────────────────────│
     │                │                │                │
```

---

## 5. Integration Points

### 5.1 Okta Integration (OIDC + SAML)

| Aspect | Configuration |
|--------|---------------|
| **Protocol** | OpenID Connect (OIDC) for SPA, SAML 2.0 for enterprise SSO |
| **Grant Type** | Authorization Code with PKCE |
| **Scopes** | openid, profile, email, groups |
| **Token Lifetime** | Access Token: 1 hour, Refresh Token: 8 hours |
| **Claims** | sub, email, name, groups (for RBAC) |
| **MFA** | Enforced via Okta policies |

**Integration Points:**
- React App: `@okta/okta-react` for login/logout, token management
- API: `Okta.AspNetCore` middleware for JWT validation
- Groups Claim: Maps to application roles for RBAC

### 5.2 Azure SQL Database Integration

| Aspect | Configuration |
|--------|---------------|
| **Connection** | Entity Framework Core 8.0 with connection pooling |
| **Authentication** | Azure AD authentication (Managed Identity) |
| **Encryption** | TLS 1.2+ in transit, TDE at rest |
| **Retry Policy** | Exponential backoff with 3 retries |
| **Connection String** | Stored in Azure Key Vault |

### 5.3 Azure Application Insights Integration

| Aspect | Configuration |
|--------|---------------|
| **SDK** | Microsoft.ApplicationInsights.AspNetCore 2.22.0 |
| **Telemetry** | Requests, dependencies, exceptions, traces, metrics |
| **Sampling** | Adaptive sampling enabled |
| **Custom Events** | Form submissions, patient operations, imports |
| **Dashboards** | Custom dashboards for operations team |

### 5.4 Foundation Data Warehouse Integration

| Aspect | Configuration |
|--------|---------------|
| **Feed Type** | Nightly batch (full and incremental) |
| **Format** | Delimited files matching DW schema |
| **Transport** | Azure Blob Storage (shared container) or SFTP |
| **Operations** | Insert, Update, Delete, Merge |
| **Reconciliation** | Row counts, checksums, error reporting |

---

## 6. Infrastructure Architecture

### 6.1 Azure Resource Topology

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              AZURE SUBSCRIPTION                                          │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                    RESOURCE GROUP: rg-ngr-prod-eastus                            │   │
│   │                                                                                   │   │
│   │   ┌─────────────────────────────────────────────────────────────────────────┐   │   │
│   │   │                    VIRTUAL NETWORK: vnet-ngr-prod                        │   │   │
│   │   │                                                                          │   │   │
│   │   │   ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐       │   │   │
│   │   │   │  Subnet: web    │   │  Subnet: api    │   │  Subnet: data   │       │   │   │
│   │   │   │  10.0.1.0/24    │   │  10.0.2.0/24    │   │  10.0.3.0/24    │       │   │   │
│   │   │   │                 │   │                 │   │                 │       │   │   │
│   │   │   │  App Service    │   │  App Service    │   │  Private        │       │   │   │
│   │   │   │  (Web App)      │   │  (API)          │   │  Endpoint       │       │   │   │
│   │   │   │                 │   │                 │   │  (SQL)          │       │   │   │
│   │   │   └─────────────────┘   └─────────────────┘   └─────────────────┘       │   │   │
│   │   │                                                                          │   │   │
│   │   └─────────────────────────────────────────────────────────────────────────┘   │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐               │   │
│   │   │  Azure Front    │   │  Azure WAF      │   │  Azure Key      │               │   │
│   │   │  Door           │   │  Policy         │   │  Vault          │               │   │
│   │   │                 │   │                 │   │                 │               │   │
│   │   │  - CDN          │   │  - OWASP 3.2    │   │  - Secrets      │               │   │
│   │   │  - SSL/TLS      │   │  - Custom Rules │   │  - Certificates │               │   │
│   │   │  - Routing      │   │  - Rate Limit   │   │  - Keys         │               │   │
│   │   └─────────────────┘   └─────────────────┘   └─────────────────┘               │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐               │   │
│   │   │  Azure SQL      │   │  Azure Blob     │   │  App Insights   │               │   │
│   │   │  Database       │   │  Storage        │   │                 │               │   │
│   │   │                 │   │                 │   │                 │               │   │
│   │   │  - Gen5 8vCore  │   │  - Hot tier     │   │  - Telemetry    │               │   │
│   │   │  - 1TB          │   │  - Lifecycle    │   │  - Logs         │               │   │
│   │   │  - Geo-backup   │   │  - Encryption   │   │  - Metrics      │               │   │
│   │   └─────────────────┘   └─────────────────┘   └─────────────────┘               │   │
│   │                                                                                   │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                    RESOURCE GROUP: rg-ngr-stage-eastus                           │   │
│   │                    (Similar structure, smaller SKUs)                              │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                    RESOURCE GROUP: rg-ngr-dev-eastus                             │   │
│   │                    (Similar structure, smallest SKUs)                             │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Environment Configuration

| Environment | App Service Plan | SQL Database | Purpose |
|-------------|------------------|--------------|---------|
| Development | B2 (Basic) | S2 (50 DTU) | Development and unit testing |
| Test | S2 (Standard) | S3 (100 DTU) | Integration testing |
| Stage/UAT | P1v3 (Premium) | GP Gen5 4vCore | User acceptance testing |
| Training | S3 (Standard) | S3 (100 DTU) | User training |
| Production | P2v3 (Premium) | GP Gen5 8vCore | Live system |

### 6.3 Network Security

- **Network Segmentation**: Each environment in separate VNet
- **Private Endpoints**: SQL Database accessible only via private endpoint
- **NSG Rules**: Restrict traffic between subnets
- **WAF**: Azure WAF with OWASP 3.2 ruleset
- **DDoS Protection**: Azure DDoS Protection Standard

---

## 7. Security Architecture

### 7.1 Authentication & Authorization

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              SECURITY ARCHITECTURE                                        │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           AUTHENTICATION LAYER                                    │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐   │   │
│   │   │  Okta           │         │  JWT Tokens     │         │  Session        │   │   │
│   │   │  Identity       │────────►│  (Access +      │────────►│  Management     │   │   │
│   │   │  Provider       │         │   ID Token)     │         │                 │   │   │
│   │   │                 │         │                 │         │  - Token Cache  │   │   │
│   │   │  - OIDC/SAML    │         │  - RS256 Signed │         │  - Refresh      │   │   │
│   │   │  - MFA          │         │  - 1hr Expiry   │         │  - Revocation   │   │   │
│   │   └─────────────────┘         └─────────────────┘         └─────────────────┘   │   │
│   │                                                                                   │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           AUTHORIZATION LAYER (RBAC)                              │   │
│   │                                                                                   │   │
│   │   ┌─────────────────────────────────────────────────────────────────────────┐   │   │
│   │   │                         ROLE HIERARCHY                                   │   │   │
│   │   │                                                                          │   │   │
│   │   │   ┌─────────────────┐                                                    │   │   │
│   │   │   │  System Admin   │  - Full system access                              │   │   │
│   │   │   │  (Foundation)   │  - User management across all programs             │   │   │
│   │   │   └────────┬────────┘  - System configuration                            │   │   │
│   │   │            │                                                             │   │   │
│   │   │   ┌────────▼────────┐                                                    │   │   │
│   │   │   │  Foundation     │  - View all patient data                           │   │   │
│   │   │   │  Analyst        │  - Run reports across programs                     │   │   │
│   │   │   └────────┬────────┘  - Content management                              │   │   │
│   │   │            │                                                             │   │   │
│   │   │   ┌────────▼────────┐                                                    │   │   │
│   │   │   │  Program Admin  │  - Manage users within program                     │   │   │
│   │   │   │  (Care Center)  │  - Full patient access within program              │   │   │
│   │   │   └────────┬────────┘  - Transfer/merge patients                         │   │   │
│   │   │            │                                                             │   │   │
│   │   │   ┌────────▼────────┐                                                    │   │   │
│   │   │   │  Clinical User  │  - View/edit patients in program                   │   │   │
│   │   │   │  (Care Center)  │  - Submit forms                                    │   │   │
│   │   │   └────────┬────────┘  - Run program reports                             │   │   │
│   │   │            │                                                             │   │   │
│   │   │   ┌────────▼────────┐                                                    │   │   │
│   │   │   │  Read-Only      │  - View patients in program                        │   │   │
│   │   │   │  User           │  - View reports                                    │   │   │
│   │   │   └─────────────────┘                                                    │   │   │
│   │   │                                                                          │   │   │
│   │   └─────────────────────────────────────────────────────────────────────────┘   │   │
│   │                                                                                   │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
│   ┌─────────────────────────────────────────────────────────────────────────────────┐   │
│   │                           DATA PROTECTION LAYER                                   │   │
│   │                                                                                   │   │
│   │   ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐               │   │
│   │   │  Encryption     │   │  Key Management │   │  Data Masking   │               │   │
│   │   │                 │   │                 │   │                 │               │   │
│   │   │  - TLS 1.2+     │   │  - Azure Key    │   │  - PHI Fields   │               │   │
│   │   │    (Transit)    │   │    Vault        │   │  - Audit Logs   │               │   │
│   │   │  - TDE (Rest)   │   │  - Managed Keys │   │  - Lower Envs   │               │   │
│   │   │  - AES-256      │   │  - Rotation     │   │                 │               │   │
│   │   └─────────────────┘   └─────────────────┘   └─────────────────┘               │   │
│   │                                                                                   │   │
│   └─────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                           │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Security Controls

| Control | Implementation |
|---------|----------------|
| **Authentication** | Okta OIDC with MFA enforcement |
| **Authorization** | Role-based access control (RBAC) with program-level scoping |
| **Encryption at Rest** | Azure SQL TDE, Blob Storage encryption |
| **Encryption in Transit** | TLS 1.2+ enforced on all endpoints |
| **Secret Management** | Azure Key Vault for all secrets, connection strings |
| **Audit Logging** | Comprehensive audit trail for all data operations |
| **WAF** | Azure WAF with OWASP 3.2 ruleset |
| **Network Security** | Private endpoints, NSGs, VNet isolation |
| **Vulnerability Scanning** | OWASP ZAP in CI/CD pipeline |
| **Code Analysis** | SonarQube for static analysis |

### 7.3 Compliance Controls

| Requirement | Implementation |
|-------------|----------------|
| **HIPAA** | PHI encryption, access controls, audit logging, BAA with Azure |
| **SOC 2 Type 2** | Security controls, monitoring, incident response |
| **Penetration Testing** | Annual third-party penetration testing |
| **Access Reviews** | Quarterly access reviews via Okta |

---

## 8. Scalability Architecture

### 8.1 Azure App Service Scaling

| Metric | Threshold | Action |
|--------|-----------|--------|
| CPU % | > 70% for 5 min | Scale out +1 instance |
| Memory % | > 80% for 5 min | Scale out +1 instance |
| HTTP Queue | > 100 requests | Scale out +1 instance |
| Response Time | > 2s avg | Scale out +1 instance |

**Scaling Limits:**
- Minimum instances: 2 (for high availability)
- Maximum instances: 10
- Scale-in cooldown: 10 minutes

### 8.2 Database Scaling

| Scenario | Strategy |
|----------|----------|
| Read-heavy workloads | Read replicas for reporting queries |
| Storage growth | Auto-grow enabled, monitor usage |
| Performance | Upgrade vCores as needed |
| Peak periods | Elastic pools for burst capacity |

### 8.3 Capacity Planning

| Metric | Current | Year 1 | Year 3 | Year 5 |
|--------|---------|--------|--------|--------|
| Active Patients | 35,000 | 36,000 | 38,000 | 40,000 |
| Encounters | 4M | 4.12M | 4.36M | 4.6M |
| Concurrent Users | 300 | 350 | 400 | 500 |
| Data Volume | 1 TB | 1.2 TB | 1.6 TB | 2 TB |

---

## 9. Deployment Architecture

### 9.1 CI/CD Pipeline

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              AZURE DEVOPS PIPELINE                                        │
├─────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                           │
│   ┌─────────────┐   ┌─────────────┐   ┌─────────────┐   ┌─────────────┐                 │
│   │   Source    │   │   Build     │   │   Test      │   │   Security  │                 │
│   │   Control   │──►│   Stage     │──►│   Stage     │──►│   Stage     │                 │
│   │             │   │             │   │             │   │             │                 │
│   │  - Git      │   │  - Restore  │   │  - xUnit    │   │  - SonarQube│                 │
│   │  - PR Gates │   │  - Build    │   │  - 70% Cov  │   │  - OWASP ZAP│                 │
│   │  - Branch   │   │  - Publish  │   │  - Playwright│  │  - Secrets  │                 │
│   │    Policy   │   │             │   │             │   │    Scan     │                 │
│   └─────────────┘   └─────────────┘   └─────────────┘   └─────────────┘                 │
│                                                               │                          │
│                                                               ▼                          │
│   ┌─────────────┐   ┌─────────────┐   ┌─────────────┐   ┌─────────────┐                 │
│   │  Production │   │   Stage     │   │   Test      │   │   Dev       │                 │
│   │   Deploy    │◄──│   Deploy    │◄──│   Deploy    │◄──│   Deploy    │                 │
│   │             │   │             │   │             │   │             │                 │
│   │  - Manual   │   │  - Auto     │   │  - Auto     │   │  - Auto     │                 │
│   │    Approval │   │  - UAT      │   │  - Int Test │   │  - Smoke    │                 │
│   │  - Blue/    │   │    Gate     │   │    Gate     │   │    Test     │                 │
│   │    Green    │   │             │   │             │   │             │                 │
│   └─────────────┘   └─────────────┘   └─────────────┘   └─────────────┘                 │
│                                                                                           │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### 9.2 Deployment Strategy

| Environment | Strategy | Approval |
|-------------|----------|----------|
| Development | Continuous deployment on merge | Automatic |
| Test | Continuous deployment | Automatic |
| Stage/UAT | Deployment on release branch | Manual (QA Lead) |
| Production | Blue/Green deployment | Manual (Release Manager) |

---

## 10. Monitoring & Observability

### 10.1 Monitoring Stack

| Component | Tool | Purpose |
|-----------|------|---------|
| APM | Azure Application Insights | Request tracing, dependencies |
| Logs | Azure Log Analytics | Centralized logging |
| Metrics | Azure Monitor | Infrastructure metrics |
| Alerts | Azure Monitor Alerts | Proactive notifications |
| Dashboards | Azure Dashboards | Operations visibility |

### 10.2 Key Metrics

| Category | Metric | Threshold |
|----------|--------|-----------|
| Availability | Uptime | > 99.9% |
| Performance | P95 Response Time | < 2 seconds |
| Performance | P99 Response Time | < 5 seconds |
| Errors | Error Rate | < 0.1% |
| Security | Failed Auth Attempts | Alert on > 10/min |
| Business | Form Submissions/Day | Baseline + variance |

### 10.3 Alerting Rules

| Alert | Condition | Severity | Action |
|-------|-----------|----------|--------|
| High Error Rate | > 1% errors for 5 min | Critical | Page on-call |
| Slow Response | P95 > 5s for 10 min | Warning | Email team |
| Database CPU | > 90% for 10 min | Warning | Email DBA |
| Failed Logins | > 50 in 5 min | Critical | Security team |
| Disk Space | > 85% used | Warning | Email ops |

---

## 11. Disaster Recovery

### 11.1 Recovery Objectives

| Metric | Target |
|--------|--------|
| RTO (Recovery Time Objective) | 4 hours |
| RPO (Recovery Point Objective) | 1 hour |

### 11.2 Backup Strategy

| Component | Backup Type | Frequency | Retention |
|-----------|-------------|-----------|-----------|
| Azure SQL | Automated | Continuous (PITR) | 35 days |
| Azure SQL | Long-term | Weekly | 1 year |
| Blob Storage | Geo-redundant | Continuous | 30 days |
| Key Vault | Soft delete | N/A | 90 days |

### 11.3 DR Procedures

1. **Database Failover**: Geo-restore to secondary region
2. **App Service**: Redeploy to secondary region via pipeline
3. **DNS Failover**: Azure Front Door automatic failover
4. **Data Validation**: Post-recovery data integrity checks

---

## 12. Appendices

### 12.1 Glossary

| Term | Definition |
|------|------------|
| NGR | Next Generation Registry |
| eCRF | Electronic Case Report Form |
| PHI | Protected Health Information |
| RBAC | Role-Based Access Control |
| OIDC | OpenID Connect |
| TDE | Transparent Data Encryption |

### 12.2 Reference Documents

- PRD-NGR-001: Product Requirements Document
- ADR-001 through ADR-006: Architecture Decision Records
- API Specification: api_specs/api.yaml
- Data Model: data_model.md
