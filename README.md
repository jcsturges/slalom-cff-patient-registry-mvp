# NGR — Next Generation Patient Registry (MVP)

The **Next Generation Patient Registry (NGR)** is a full re-platforming of PortCF, the Cystic Fibrosis Foundation's patient registry that has tracked ~50,000 CF patients (~80% of all US CF patients) since the 1960s. NGR replaces an aging, inflexible system with a modern, secure, cloud-native platform built for extensibility.

This repository contains the **MVP artifacts** — produced by a 5-stage AI-driven SDLC pipeline built on AWS Bedrock AgentCore that converts the CFF RFP into working software artifacts. The pipeline generated **102 artifacts** across PRD, architecture, source code, tests, and infrastructure-as-code.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Tech Stack](#tech-stack)
- [Repository Structure](#repository-structure)
- [Key Documents](#key-documents)
- [Prerequisites](#prerequisites)
- [Local Development Setup](#local-development-setup)
- [Hot Reload Development](#step-7--hot-reload-development-without-docker-compose)
- [Okta Configuration](#okta-configuration)
- [Azure Infrastructure Provisioning](#azure-infrastructure-provisioning)
- [CI/CD Pipeline](#cicd-pipeline)
- [API Reference](#api-reference)
- [Data Model](#data-model)
- [Architecture Decision Records](#architecture-decision-records)
- [Operations & Monitoring](#operations--monitoring)
- [Known Limitations](#known-limitations)

---

## Architecture Overview

NGR follows a three-tier architecture deployed on Microsoft Azure:

```
┌─────────────────────────────────────────────────────────────────┐
│                        Azure Front Door + WAF                   │
│                    (optional, production only)                  │
└────────────────┬─────────────────────────┬──────────────────────┘
                 │                         │
    ┌────────────▼──────────┐  ┌───────────▼──────────┐
    │   Azure App Service   │  │   Azure App Service  │
    │   (React 18 SPA)      │  │   (ASP.NET Core 8)   │
    │   Node 20 / nginx     │  │   .NET 8 Runtime     │
    │   Port 8080           │  │   Port 8080          │
    │   Subnet: Web         │  │   Subnet: API        │
    └───────────────────────┘  └──────────┬───────────┘
                                          │
                               ┌──────────▼─────────────┐
                               │   Azure SQL Database   │
                               │   EF Core 8 / T-SQL    │
                               │   Subnet: Data         │
                               │   (Private Endpoint)   │
                               └────────────────────────┘

    Supporting Services:
    ├── Azure Key Vault .............. Secrets management
    ├── Azure Application Insights ... Telemetry & monitoring
    ├── Azure Blob Storage ........... File uploads, exports, DW feed
    ├── Azure Container Registry ..... Docker image registry
    └── Okta ......................... SSO / OIDC / JWT authentication
```

**Network topology:** A single VNet (`10.0.0.0/16`) with three subnets — Web (`10.0.1.0/24`), API (`10.0.2.0/24`), and Data (`10.0.3.0/24`). The API subnet has App Service delegation; the Data subnet uses service endpoints for SQL.

**Authentication flow:** Users authenticate via Okta OIDC (PKCE). The React SPA obtains a JWT access token and passes it as a Bearer token to the API. The API validates the token against Okta's authorization server and enforces role-based access control (RBAC) with four roles:

| Role | Access |
|------|--------|
| **SystemAdmin** | Full system access |
| **FoundationAnalyst** | Foundation staff read access |
| **ProgramAdmin** | Care program administration |
| **ClinicalUser** | Clinical staff with patient access |

---

## Tech Stack

Every technology choice is dictated by the CFF RFP and cannot be changed.

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | React + TypeScript (Vite, Material-UI, React Query) | 18.x |
| API | ASP.NET Core Web API (C# 12) | .NET 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | Azure SQL Database | — |
| Auth | Okta (OIDC/SAML, JWT Bearer) | — |
| Validation | FluentValidation | 11.9 |
| Mapping | AutoMapper | 12.0 |
| Logging | Serilog → Console + Application Insights | 8.0 |
| Hosting | Azure App Services (Linux) | — |
| CI/CD | Azure DevOps YAML Pipelines | — |
| E2E Testing | Playwright (TypeScript) | — |
| Unit Testing | xUnit (.NET) | — |
| Code Quality | SonarQube + OWASP ZAP | — |
| Monitoring | Azure Application Insights + Azure WAF | — |
| IaC | Terraform (hashicorp/azurerm) | >= 1.6.0, ~> 3.85 |
| Containers | Docker (multi-stage builds) | — |

---

## Repository Structure

This repository was produced by pipeline run ID `271c9944-1e04-4d3d-91b9-9c0d9b99a354`.

```
patient-registry-mvp/
├── CLAUDE.md                          # Claude Code project configuration
├── CLAUDE_CONTEXT.md                  # Detailed project context document
├── README.md                          # This file
│
├── prd/                    # Business Analysis outputs
│   ├── prd.md                         #   Full Product Requirements Document
│   ├── user_stories.md                #   Epics and user stories
│   ├── acceptance_criteria.md         #   Acceptance criteria per feature
│   ├── ambiguities.md                 #   Open questions / assumptions
│   └── prd_summary_for_architect.md   #   Technical handoff summary
│
├── architecture/           # Architecture outputs
│   ├── system_design.md               #   C4 model, deployment topology
│   ├── data_model.md                  #   Database schema specification
│   ├── api_specs/api.yaml             #   OpenAPI 3.0 specification
│   ├── components.json                #   Component manifest
│   └── adrs/                          #   Architecture Decision Records (38 files)
│
├── src/                    # Source code (61 files)
│   ├── ngr-api/                       #   ASP.NET Core 8 REST API
│   │   ├── Program.cs                 #     App bootstrapping, DI, middleware
│   │   ├── NGR.Api.csproj             #     Project file, NuGet dependencies
│   │   ├── Controllers/               #     API controllers
│   │   ├── Models/                    #     Domain entities
│   │   ├── DTOs/                      #     Data transfer objects
│   │   ├── Services/                  #     Business logic layer
│   │   ├── Validators/                #     FluentValidation rules
│   │   ├── Middleware/                #     Error handling, request logging
│   │   ├── Mapping/                   #     AutoMapper profiles
│   │   └── Data/                      #     ApplicationDbContext
│   │
│   ├── ngr-web-app/                   #   React 18 TypeScript SPA
│   │   └── src/
│   │       ├── App.tsx                #     Root component (Okta, React Query, MUI)
│   │       ├── main.tsx               #     Entry point
│   │       ├── theme.ts              #     Material-UI theme
│   │       └── components/            #     UI components
│   │
│   └── ngr-database/                  #   Database project
│       ├── NgrDbContext.cs            #     EF Core DbContext (21 entities)
│       ├── Ngr.Database.csproj        #     Project file
│       ├── Entities/                  #     EF Core entity classes (23 files)
│       └── Migrations/               #     SQL migration scripts
│           ├── 001_InitialSchema.sql  #       Schema creation (T-SQL)
│           └── 002_SeedData.sql       #       Reference data
│
├── tests/                  # Test suite
│   └── api/UnitTests/Controllers/
│       └── PatientsControllerTests.cs #   xUnit controller tests
│
├── iac/                    # Infrastructure as Code (32 files)
│   ├── terraform/                     #   Terraform modules
│   │   ├── main.tf                    #     Root module (all Azure resources)
│   │   ├── variables.tf               #     Input variables
│   │   ├── outputs.tf                 #     Output values
│   │   ├── provider.tf                #     Provider + backend config
│   │   ├── environments/              #     Per-environment tfvars
│   │   │   ├── dev.tfvars
│   │   │   ├── test.tfvars
│   │   │   ├── stage.tfvars
│   │   │   └── prod.tfvars
│   │   └── modules/                   #     Reusable Terraform modules
│   │       ├── acr/                   #       Azure Container Registry
│   │       ├── app-service/           #       App Service + Plan
│   │       ├── appinsights/           #       Application Insights
│   │       ├── frontdoor/             #       Front Door + WAF
│   │       ├── keyvault/              #       Key Vault
│   │       └── sql/                   #       SQL Server + Database
│   │
│   ├── cicd/
│   │   └── azure-pipelines.yml        #   Full CI/CD pipeline (7 stages)
│   │
│   ├── dockerfiles/
│   │   ├── api.Dockerfile             #   Multi-stage .NET 8 build
│   │   ├── web-app.Dockerfile         #   Node 20 build → nginx
│   │   └── .dockerignore
│   │
│   ├── docker-compose.yml             #   Local dev: SQL + API + Web
│   └── runbook.md                     #   Ops runbook
│
└── reports/
    └── delivery_report.json           # Pipeline completion manifest
```

---

## Key Documents

| Document | Path | Description |
|----------|------|-------------|
| Product Requirements | [`prd/prd.md`](prd/prd.md) | Full PRD with functional and non-functional requirements |
| User Stories | [`prd/user_stories.md`](prd/user_stories.md) | Epics and user stories with acceptance criteria |
| System Design | [`architecture/system_design.md`](architecture/system_design.md) | C4 diagrams, deployment topology, component architecture |
| Data Model | [`architecture/data_model.md`](architecture/data_model.md) | Full database schema, indexes, retention policies |
| API Specification | [`architecture/api_specs/api.yaml`](architecture/api_specs/api.yaml) | OpenAPI 3.0 spec for all endpoints |
| Operations Runbook | [`iac/runbook.md`](iac/runbook.md) | Deploy, rollback, scaling, monitoring, incident response |
| CI/CD Pipeline | [`iac/cicd/azure-pipelines.yml`](iac/cicd/azure-pipelines.yml) | 7-stage Azure DevOps pipeline |
| Ambiguities | [`prd/ambiguities.md`](prd/ambiguities.md) | Open questions and assumptions made during generation |

---

## Prerequisites

### Local Development

| Dependency | Version | Purpose |
|-----------|---------|---------|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Latest | Runs Azure SQL Edge, API, and Web containers |
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0+ | Building/debugging the API outside Docker |
| [Node.js](https://nodejs.org/) | 20 LTS | Building/debugging the React app outside Docker |
| [Okta Developer Account](https://developer.okta.com/) | — | OIDC authentication (see [Okta Configuration](#okta-configuration)) |

> **Platform support:** Local dev is verified on Apple Silicon (M-series) Macs. The docker-compose uses Azure SQL Edge (ARM64-native) instead of SQL Server 2022 (amd64-only). Intel Macs and Linux x86_64 will also work with Azure SQL Edge.

### Azure Deployment

| Dependency | Version | Purpose |
|-----------|---------|---------|
| [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) | Latest | Azure resource management |
| [Terraform](https://developer.hashicorp.com/terraform/install) | >= 1.6.0 | Infrastructure provisioning |
| Azure Subscription | — | With permissions to create resources |
| [Azure DevOps](https://dev.azure.com/) | — | CI/CD pipeline execution |

---

## Local Development Setup

### Step 1 — Prerequisites

Ensure the following are installed:

- **Docker Desktop** (latest) — [docker.com](https://www.docker.com/products/docker-desktop/)
- **Okta developer account** — needed for authentication ([developer.okta.com](https://developer.okta.com/))
- Optional for native dev: .NET 8 SDK, Node.js 20 LTS

### Step 2 — Configure Okta Credentials

Create a `.env` file alongside `docker-compose.yml` with your Okta app values:

```bash
# iac/.env
OKTA_AUTHORITY=https://dev-YOUR_ORG.okta.com/oauth2/default
OKTA_CLIENT_ID=your_okta_client_id
```

> The docker-compose.yml has default placeholder values that will allow the stack to start, but authentication won't work until real Okta credentials are supplied. See [Okta Configuration](#okta-configuration) for setup steps.

### Step 3 — Start All Services

```bash
cd iac
docker-compose up --build -d
```

This builds and starts three containers in order:

| Service | URL | Notes |
|---------|-----|-------|
| **Azure SQL Edge** | `localhost:1433` | ARM64-native SQL Server substitute. SA password: `Dev!Password123` |
| **ASP.NET Core API** | `http://localhost:5000` | Waits for SQL health check. Swagger at `/swagger` |
| **React SPA** | `http://localhost:3000` | Waits for API health check. nginx on port 8080 internally |

First build takes 2–5 minutes (downloads base images, restores NuGet packages, runs `npm ci`). Subsequent builds are cached.

> **Apple Silicon (M-series):** Azure SQL Edge is used instead of SQL Server 2022, which only has amd64 images and crashes on ARM64.

### Step 4 — Database is Auto-Created

No manual migration step is needed. When the API starts in `Development` mode, it calls EF Core `EnsureCreated()` which automatically creates the `PatientRegistryDev` database and all tables.

The tables are created in the `dbo` schema (EF Core default). They will be empty — use the API endpoints or Swagger UI to create test records.

> The raw SQL migration scripts at `src/ngr-database/Migrations/` use a different `ngr` schema and are intended for production Azure SQL deployment only.

### Step 5 — Verify Everything is Running

```bash
# Check all containers are healthy
docker-compose ps

# API health endpoint (no auth required)
curl http://localhost:5000/health
# → {"status":"healthy","timestamp":"2026-02-26T..."}

# Swagger UI (open in browser)
open http://localhost:5000/swagger

# Web app (open in browser — redirects to Okta login)
open http://localhost:3000

# Web app health file
curl http://localhost:3000/health.txt
# → ok
```

### Step 6 — Stopping and Restarting

```bash
# Stop containers (preserves database volume)
docker-compose down

# Stop and wipe database (full reset)
docker-compose down -v

# Restart without rebuilding images
docker-compose up -d

# Rebuild and restart (after code changes)
docker-compose up --build -d
```

### Step 7 — Hot Reload Development (without docker-compose)

For active API or frontend development, run the API and web app natively so changes apply instantly. The database still runs in Docker; only the two application processes run on your host.

#### 7a — Start the Database Only

```bash
cd iac

# Start only the SQL container (skip the API and web-app containers)
docker-compose up -d sql

# Wait for it to be healthy (~15-20 seconds)
docker-compose ps
```

You should see `sql` with status `healthy` before proceeding.

#### 7b — Run the API with Hot Reload

`appsettings.Development.json` already points to `localhost:1433`, so no extra configuration is needed.

```bash
cd src/ngr-api

# Hot reload — recompiles and restarts on every file save
dotnet watch run
```

The API starts at **http://localhost:5000**. `dotnet watch` monitors all `.cs` files and triggers a rebuild automatically on save. Swagger UI is at `http://localhost:5000/swagger`.

> **First run:** EF Core `EnsureCreated()` creates the `PatientRegistryDev` database and tables automatically on startup.

> **Override credentials (optional):** If you need to point to a different SQL instance, use .NET User Secrets rather than editing `appsettings.Development.json`:
> ```bash
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
>   "Server=localhost,1433;Database=PatientRegistryDev;User Id=sa;Password=Dev!Password123;TrustServerCertificate=True"
> ```

#### 7c — Run the React App with Hot Reload

Vite's dev server provides Hot Module Replacement (HMR) — React components update in the browser without a full page reload.

**One-time setup:** Create a `.env.local` file in the `ngr-web-app/` directory (gitignored) with your environment values:

```bash
cd src/ngr-web-app

cat > .env.local << 'EOF'
VITE_API_URL=http://localhost:5000
VITE_OKTA_ISSUER=https://dev-YOUR_ORG.okta.com/oauth2/default
VITE_OKTA_CLIENT_ID=your_client_id
VITE_OKTA_REDIRECT_URI=http://localhost:3000/login/callback
EOF
```

**Then install dependencies and start the dev server:**

```bash
npm install        # only needed on first run or after package.json changes
npm run dev
```

The app starts at **http://localhost:3000** with full HMR. Changes to `.tsx`, `.ts`, and `.css` files apply instantly in the browser.

> **CORS:** `appsettings.Development.json` already allows `http://localhost:3000` — the API will accept requests from the Vite dev server without any changes.

#### 7d — Hot Reload Summary

| Process | Command | URL | Reload |
|---------|---------|-----|--------|
| Database | `docker-compose up -d sql` | `localhost:1433` | — |
| API | `dotnet watch run` | `http://localhost:5000` | Auto on `.cs` save |
| Web App | `npm run dev` | `http://localhost:3000` | HMR on `.tsx/.ts` save |

#### 7e — Stopping the Database

```bash
cd iac

# Stop the SQL container (preserves data volume)
docker-compose stop sql

# Or wipe the database for a clean slate
docker-compose down -v
```

### Step 8 — Connecting to the Database

```
Host:     localhost
Port:     1433
Username: sa
Password: Dev!Password123
Database: PatientRegistryDev
Schema:   dbo
```

Recommended clients: **Azure Data Studio** (free, cross-platform) or **DataGrip**.

> Azure SQL Edge does not bundle `sqlcmd` inside the container. Use a host-installed client to connect.

---

## Okta Configuration

NGR uses Okta for all authentication. You need an Okta developer tenant to run locally.

### 1. Create an Okta Developer Account

Sign up at [developer.okta.com](https://developer.okta.com/) if you don't have an account.

### 2. Create an OIDC Application

1. Go to **Applications** → **Create App Integration**
2. Select **OIDC - OpenID Connect** and **Single-Page Application**
3. Configure:
   - **App name:** `NGR Local Dev`
   - **Grant type:** Authorization Code (with PKCE)
   - **Sign-in redirect URIs:** `http://localhost:3000/login/callback`
   - **Sign-out redirect URIs:** `http://localhost:3000`
   - **Trusted Origins:** `http://localhost:3000`
4. Note the **Client ID** (you will not need a client secret for SPA — PKCE is used)

### 3. Create an API Authorization Server (or use `default`)

1. Go to **Security** → **API** → **Authorization Servers**
2. Use the `default` server or create a new one
3. Add a custom scope if needed (the app requests: `openid profile email groups`)
4. Add a claim for `groups` if using group-based RBAC:
   - **Name:** `groups`
   - **Include in:** Access Token
   - **Value type:** Groups
   - **Filter:** Matches regex `.*`

### 4. Create Test Users with Roles

Create users and assign them to groups matching the four RBAC roles:

- `SystemAdmin`
- `FoundationAnalyst`
- `ProgramAdmin`
- `ClinicalUser`

### 5. Configure Your Environment

Use the values from steps 2-3 to populate:

| Variable | Example Value |
|----------|--------------|
| `VITE_OKTA_ISSUER` | `https://dev-12345.okta.com/oauth2/default` |
| `VITE_OKTA_CLIENT_ID` | `0oa1bcdef2ghijklm3n4` |
| `VITE_OKTA_REDIRECT_URI` | `http://localhost:3000/login/callback` |
| `Okta:Authority` (API) | `https://dev-12345.okta.com/oauth2/default` |
| `Okta:Audience` (API) | `api://ngr` |

---

## Azure Infrastructure Provisioning

All Azure infrastructure is defined in Terraform under `iac/terraform/`.

### Resources Provisioned

| Resource | Naming Convention | Purpose |
|----------|------------------|---------|
| Resource Group | `rg-ngr-{env}-{region}` | Container for all resources |
| Virtual Network | `vnet-ngr-{env}` | Network isolation (10.0.0.0/16) |
| App Service Plan | `asp-ngr-{env}` | Linux compute plan |
| API App Service | `app-ngr-{env}-api` | .NET 8 API hosting |
| Web App Service | `app-ngr-{env}-web` | React SPA hosting (Node 20) |
| Azure SQL Server | `sql-ngr-{env}` | Database server |
| Azure SQL Database | `sqldb-ngr-{env}` | Patient registry database |
| Key Vault | `kv-ngr-{env}` | Secrets (connection strings, Okta) |
| Storage Account | `stngr{env}` | Blob storage (uploads, exports, DW feed) |
| Container Registry | `acrngr{env}` | Docker image registry |
| Application Insights | `appi-ngr-{env}` | Telemetry and monitoring |
| Front Door + WAF | `fd-ngr-{env}` | CDN + WAF (optional, production) |

### Environment Configurations

Four environments are pre-configured in `terraform/environments/`:

| Environment | SKU (App) | SKU (SQL) | SQL Size | Auto-scale | WAF |
|------------|-----------|-----------|----------|------------|-----|
| **dev** | B2 | S2 | 50 GB | 1-2 instances | No |
| **test** | B2 | S2 | 50 GB | 1-2 instances | No |
| **stage** | P1V3 | S3 | 100 GB | 2-5 instances | Yes |
| **prod** | P1V3 | P2 | 500 GB | 2-10 instances | Yes |

### Step 1: Set Up Terraform Backend

Before first use, create the Terraform state storage account:

```bash
az group create --name rg-ngr-tfstate --location eastus

az storage account create \
  --name stngrterraformstate \
  --resource-group rg-ngr-tfstate \
  --sku Standard_LRS \
  --encryption-services blob

az storage container create \
  --name tfstate \
  --account-name stngrterraformstate
```

### Step 2: Initialize Terraform

```bash
cd iac/terraform

# Login to Azure
az login

terraform init
```

### Step 3: Plan and Apply

```bash
# Dev environment
terraform plan \
  -var-file=environments/dev.tfvars \
  -var="sql_admin_password=YOUR_SECURE_PASSWORD" \
  -var="okta_client_id=YOUR_OKTA_CLIENT_ID" \
  -var="okta_client_secret=YOUR_OKTA_CLIENT_SECRET"

terraform apply \
  -var-file=environments/dev.tfvars \
  -var="sql_admin_password=YOUR_SECURE_PASSWORD" \
  -var="okta_client_id=YOUR_OKTA_CLIENT_ID" \
  -var="okta_client_secret=YOUR_OKTA_CLIENT_SECRET"
```

Alternatively, use environment variables for sensitive values:

```bash
export TF_VAR_sql_admin_password="YOUR_SECURE_PASSWORD"
export TF_VAR_okta_client_id="YOUR_OKTA_CLIENT_ID"
export TF_VAR_okta_client_secret="YOUR_OKTA_CLIENT_SECRET"

terraform apply -var-file=environments/dev.tfvars
```

### Step 4: Retrieve Outputs

After provisioning:

```bash
terraform output

# Key outputs:
# api_app_service_url   = "https://app-ngr-dev-api.azurewebsites.net"
# web_app_service_url   = "https://app-ngr-dev-web.azurewebsites.net"
# key_vault_uri         = "https://kv-ngr-dev.vault.azure.net/"
# acr_login_server      = "acrngrdev.azurecr.io"
# sql_server_name       = "sql-ngr-dev"
```

### Step 5: Deploy Application

After infrastructure is provisioned, build and push Docker images:

```bash
# Login to ACR
az acr login --name acrngrdev

# Build and push API
docker build -f iac/dockerfiles/api.Dockerfile \
  -t acrngrdev.azurecr.io/ngr-api:latest \
  src/ngr-api/
docker push acrngrdev.azurecr.io/ngr-api:latest

# Build and push Web App
docker build -f iac/dockerfiles/web-app.Dockerfile \
  -t acrngrdev.azurecr.io/ngr-web-app:latest \
  src/ngr-web-app/
docker push acrngrdev.azurecr.io/ngr-web-app:latest

# Update App Services to use the new images
az webapp config container set \
  --name app-ngr-dev-api \
  --resource-group rg-ngr-dev-eastus \
  --container-image-name acrngrdev.azurecr.io/ngr-api:latest

az webapp config container set \
  --name app-ngr-dev-web \
  --resource-group rg-ngr-dev-eastus \
  --container-image-name acrngrdev.azurecr.io/ngr-web-app:latest
```

### Production Considerations

- **Enable WAF:** Set `enable_waf = true` in `prod.tfvars`
- **Private Endpoints:** Set `enable_private_endpoint = true` for SQL Server
- **Auto-scaling:** Configured automatically for prod/stage (CPU > 70% or Memory > 80%)
- **Blob Storage:** Production uses GRS (geo-redundant) replication; lower environments use LRS
- **Key Vault:** Soft delete and purge protection enabled; API + Web use Managed Identity for access

---

## CI/CD Pipeline

The Azure DevOps pipeline (`iac/cicd/azure-pipelines.yml`) has 7 stages:

```
┌─────────────┐   ┌──────────┐   ┌──────────────┐   ┌───────────────┐
│  1. Build   │──▶│ 2. Test  │──▶│ 3. SonarQube │──▶│ 4. OWASP ZAP  │
│ (API + Web) │   │ (xUnit)  │   │ (Quality)    │   │ (Security)    │
└─────────────┘   └──────────┘   └──────────────┘   └───────┬───────┘
                                                             │
                  ┌──────────┐   ┌──────────────┐   ┌───────▼───────┐
                  │ 7. Prod  │◀──│ 6. E2E Tests │◀──│ 5. Deploy Dev │
                  │ (Manual) │   │ (Playwright) │   │ (Auto)        │
                  └──────────┘   └──────────────┘   └───────────────┘
```

| Stage | Trigger | Description |
|-------|---------|-------------|
| **Build** | All commits | Parallel build of API (.NET 8) and Web (Node 20), Docker images pushed to ACR |
| **Test** | After build | xUnit tests with Cobertura code coverage |
| **SonarQube** | After test | Static analysis and code quality gates |
| **OWASP ZAP** | After SonarQube | Security baseline scan against deployed dev |
| **Deploy Dev** | main branch only | Auto-deploy to dev App Services |
| **E2E Tests** | After dev deploy | Playwright tests with Okta credentials |
| **Deploy Prod** | Manual gate | Manual approval required, slot swap for zero-downtime |

**Container Registry:** `ngrregistry.azurecr.io`

**Required Pipeline Variables** (set in Azure DevOps variable groups):
- `OKTA_AUTHORITY`, `OKTA_AUDIENCE`
- `SONAR_PROJECT_KEY`
- `ZAP_TARGET_URL`
- SQL and Okta connection credentials

---

## API Reference

The full OpenAPI 3.0 specification is at [`architecture/api_specs/api.yaml`](architecture/api_specs/api.yaml).

**Base URLs:**

| Environment | URL |
|------------|-----|
| Local Dev | `http://localhost:5000` |
| Dev | `https://api-dev.ngr.cff.org/v1` |
| Staging | `https://api-stage.ngr.cff.org/v1` |
| Production | `https://api.ngr.cff.org/v1` |

**Resource Groups:**

| Tag | Endpoints | Description |
|-----|-----------|-------------|
| Authentication | `/auth/*` | User info, session management |
| Patients | `/patients/*` | Roster CRUD, search, transfer, merge |
| Encounters | `/encounters/*` | Encounter lifecycle management |
| Forms | `/forms/*` | eCRF definitions, submissions, auto-save |
| Reports | `/reports/*` | Dynamic queries, standard reports, exports |
| Imports | `/imports/*` | CSV batch upload and validation |
| Content | `/content/*` | Help articles, announcements |
| Users | `/users/*` | User management and role assignment |
| Programs | `/programs/*` | Care program administration |

**Health Check:** `GET /health` (unauthenticated, returns `{"status":"healthy","timestamp":"..."}`)

---

## Data Model

The database uses 21 entity types. The SQL migration scripts create tables in the `ngr` schema; the API's EF Core `ApplicationDbContext` uses the default `dbo` schema (14 entities). Full specification is in [`architecture/data_model.md`](architecture/data_model.md).

### Core Tables

| Category | Tables | Year 1 Estimate |
|----------|--------|-----------------|
| **Lookup** | Roles, EncounterTypes, FormStatuses | Static reference data |
| **Programs** | CarePrograms, Users, ProgramUsers | 136 centers, 3,000+ users |
| **Patients** | Patients, PatientPrograms, PatientDemographics, PatientMergeHistory | 36,000 patients, 50 MB |
| **Clinical** | Encounters, FormDefinitions, FormSubmissions, FormFieldHistory | 4.1M encounters, 8M submissions, ~500 GB |
| **Import** | ImportJobs, ImportErrors | CSV upload tracking |
| **Content** | Contents, Announcements | CMS for foundation-published content |
| **Reports** | SavedReports, StandardReports | User-created and pre-built reports |
| **Audit** | AuditLogs, DataWarehouseSyncStatus, ChangeTracking | 50M audit rows/year, ~100 GB |

### Retention Policies

| Data Type | Retention | Storage |
|-----------|-----------|---------|
| Patient / Encounter / Form | Indefinite | Regulatory requirement |
| Audit Logs | 7 years | 1 year hot, 6 years cold |
| Import Jobs | 90 days | — |
| Change Tracking | 30 days | — |

---

## Architecture Decision Records

ADRs are located in `architecture/adrs/`. Key decisions:

| # | Decision | Choice | Rationale |
|---|----------|--------|-----------|
| 001 | Cloud Platform | Azure | CFF Microsoft ecosystem alignment |
| 002 | Backend Framework | ASP.NET Core 8 | RFP requirement, C# 12 |
| 003 | Database | Azure SQL | Relational model fits patient/encounter/form structure; HIPAA-ready |
| 004 | Frontend | React 18 TypeScript | RFP requirement |
| 005 | Hosting | Azure App Services | Lower ops overhead than AKS for MVP |
| 006 | Authentication | Okta OIDC | CFF has existing Okta instance |
| 007 | Form Storage | JSONB columns | Flexible schema for dynamic eCRF data |
| 008 | API Style | RESTful | Simpler for MVP, extensible later |
| 009 | Caching | Redis (future) | Not implemented in MVP |
| 010 | IaC | Terraform | RFP requirement, azurerm provider |

---

## Operations & Monitoring

See the full operations runbook at [`iac/runbook.md`](iac/runbook.md).

### Service Endpoints

| Environment | API | Web | SQL |
|------------|-----|-----|-----|
| Dev | `https://ngr-api-dev.azurewebsites.net` | `https://ngr-web-dev.azurewebsites.net` | Private endpoint |
| Stage | `https://ngr-api-stage.azurewebsites.net` | `https://ngr-web-stage.azurewebsites.net` | Private endpoint |
| Prod | `https://ngr-api-prod.azurewebsites.net` | `https://ngr-web-prod.azurewebsites.net` | Private endpoint |

### Production Alert Thresholds

| Alert | Threshold | Action |
|-------|-----------|--------|
| 5xx Error Rate | > 1% / 5 min | PagerDuty |
| API p95 Latency | > 2000ms / 5 min | Email |
| SQL DTU Usage | > 80% / 10 min | Scale up |
| App CPU | > 70% / 15 min | Auto-scale |
| Failed Logins | > 50/min | Security team |

### Rollback

- **Fastest:** Slot swap — `az webapp deployment slot swap`
- **Image rollback:** `az webapp config container set` with previous build ID tag

---

## Known Limitations

This MVP was generated by an AI pipeline as a demonstration. The following items need attention before production:

| Limitation | Impact | Action Required |
|-----------|--------|-----------------|
| **Thin test coverage** | 1 test file exists | Expand to 70%+ per NFR requirement |
| **Approximated business rules** | Form engine, RBAC, ownership rules are structural | Refine against Appendix A/C/E when available |
| **No data migration ETL** | Data model exists but no PortCF migration tooling | Build ETL pipeline separately |
| **Stubbed form specs** | Form engine needs real eCRF definitions | Awaiting CFF Appendix C machine-readable specs |
| **Placeholder secrets** | All connection strings are placeholders | Wire to Azure Key Vault for real deployment |
| **ADR duplicates** | Multiple ADR versions exist for some decisions | Consolidate to canonical set |
| **Dual DbContext** | `ApplicationDbContext` (API, `dbo`) vs `NgrDbContext` (database project, `ngr`) | Consolidate to single context |
| **Schema mismatch** | SQL migration scripts use `ngr` schema; EF Core uses `dbo` | Align before production deployment |
| **Azure.Identity vulnerability** | Pinned at 1.10.4 with moderate CVEs | Upgrade to latest before production |

---

## How This MVP Was Generated

This repository was produced by a **5-stage AI-driven SDLC pipeline** built on **AWS Bedrock AgentCore**:

```
RFP PDF → [BA Agent] → [Architect Agent] → [Engineer Agent] → [QA Agent] → [DevOps Agent]
                                                                                   ↓
                                             102 artifacts across PRD, architecture, source code, tests, IaC
```

Each agent is a **Strands-based Claude Opus 4.5 agent** running on AgentCore, communicating via agent-to-agent (A2A) invocation through an MCP Gateway backed by Lambda tools.

- **Pipeline Run ID:** `271c9944-1e04-4d3d-91b9-9c0d9b99a354`
- **Completed:** February 24, 2026
- **S3 Artifacts:** `s3://cff-ngr-mvp/`
- **Pipeline Source:** Separate repository (`rfp-to-mvp/`)
