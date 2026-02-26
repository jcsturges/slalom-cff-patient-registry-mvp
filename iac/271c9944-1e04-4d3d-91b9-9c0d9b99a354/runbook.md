# PatientRegistry — Operations Runbook

## Service Overview
| Component | Azure Resource | URL |
|---|---|---|
| API | App Service: ngr-api-{env} | https://ngr-api-{env}.azurewebsites.net |
| Web App | App Service: ngr-web-app-{env} | https://ngr-web-{env}.azurewebsites.net |
| Database | Azure SQL: ngr-sql-{env} | Private endpoint only |
| CDN/WAF | Azure Front Door | https://ngr.yourdomain.com |
| Auth | Okta | https://your-org.okta.com |
| Monitoring | Application Insights | Azure Portal → ngr-insights-{env} |

---

## 1. Deploying to Each Environment

### Deploy to Dev
Triggered automatically on merge to `main` via Azure DevOps pipeline.
- Branch: `main`
- Stage: `Deploy_Dev` (auto-approves)
- App Services: `ngr-api-dev`, `ngr-web-app-dev`

### Deploy to Production
Requires manual approval gate in Azure DevOps.
```bash
# 1. Ensure E2E tests and security scan have passed (pipeline runs automatically)
# 2. Navigate to Azure DevOps → Pipelines → PatientRegistry
# 3. Find the run awaiting approval on Deploy_Prod stage
# 4. Click "Review" → "Approve"
# 5. Monitor deployment in Azure Portal → App Services
```

### Manual Hotfix Deploy (Emergency Only)
```bash
# Build and push hotfix image
docker build -f iac/dockerfiles/api.Dockerfile -t ngrregistry.azurecr.io/ngr-api:hotfix-$(date +%Y%m%d) src/ngr-api/
az acr login --name ngrregistry
docker push ngrregistry.azurecr.io/ngr-api:hotfix-$(date +%Y%m%d)

# Deploy to prod App Service
az webapp config container set \
  --name ngr-api-prod \
  --resource-group rg-ngr-prod \
  --docker-custom-image-name ngrregistry.azurecr.io/ngr-api:hotfix-$(date +%Y%m%d)
```

---

## 2. Rolling Back

### Slot Swap Rollback (< 1 min)
If deployment slots are configured (recommended for production):
```bash
# Swap back to previous slot
az webapp deployment slot swap \
  --name ngr-api-prod \
  --resource-group rg-ngr-prod \
  --slot staging \
  --target-slot production
```

### Image Tag Rollback
```bash
# Find previous working build ID
az webapp config container show \
  --name ngr-api-prod \
  --resource-group rg-ngr-prod \
  --query '[].{image:value}' -o table

# Roll back to previous image
az webapp config container set \
  --name ngr-api-prod \
  --resource-group rg-ngr-prod \
  --docker-custom-image-name ngrregistry.azurecr.io/ngr-api:<PREV_BUILD_ID>
```

---

## 3. Scaling the App Service Plan

### Scale Up (Vertical — Change SKU)
```bash
az appservice plan update \
  --name ngr-plan-prod \
  --resource-group rg-ngr-prod \
  --sku P2V3
# SKUs: B1/B2/B3 (Basic), P1V3/P2V3/P3V3 (Premium v3)
```

### Scale Out (Horizontal — Increase Instances)
```bash
az monitor autoscale create \
  --resource-group rg-ngr-prod \
  --resource ngr-plan-prod \
  --resource-type Microsoft.Web/serverFarms \
  --name ngr-autoscale \
  --min-count 2 --max-count 10 --count 2

# Manual scale
az appservice plan update \
  --name ngr-plan-prod \
  --resource-group rg-ngr-prod \
  --number-of-workers 4
```

---

## 4. Key Application Insights Dashboards and Alerts

### Recommended Dashboards (Azure Portal)
1. **Application Map** — shows API → SQL dependencies and failure rates
2. **Live Metrics** — real-time request rate, failures, server health
3. **Failures** — exception drill-down by operation
4. **Performance** — p95/p99 latency by endpoint

### Critical Alerts to Configure
| Alert | Threshold | Action |
|---|---|---|
| 5xx Error Rate | > 1% over 5 min | PagerDuty |
| API Latency p95 | > 2000ms over 5 min | Email |
| SQL DTU | > 80% for 10 min | Scale-up App Service Plan |
| App Service CPU | > 70% for 15 min | Auto-scale |
| Failed Logins (Okta) | > 50/min | Security team |

### KQL Queries — Application Insights
```kql
// Top 10 slowest endpoints (last 24h)
requests
| where timestamp > ago(24h)
| summarize avg_duration=avg(duration), p95=percentile(duration, 95), count=count()
  by name
| order by p95 desc
| take 10

// Patient API error rate by operation
requests
| where timestamp > ago(1h)
| where name startswith "PatientsController"
| summarize error_rate=countif(success==false)*100.0/count() by name
| order by error_rate desc

// SQL call duration anomalies
dependencies
| where timestamp > ago(1h)
| where type == "SQL"
| where duration > 1000
| project timestamp, name, duration, success, data
| order by duration desc
```

---

## 5. Incident Response — Top 5 Failure Modes

### Failure 1: API 503 — App Service Unavailable
**Symptoms:** All API calls return 503, Front Door health check failing
**Diagnosis:**
```bash
az webapp show --name ngr-api-prod --resource-group rg-ngr-prod --query state
az webapp log tail --name ngr-api-prod --resource-group rg-ngr-prod
```
**Resolution:**
1. Check App Service Plan CPU/memory — scale out if saturated
2. Restart app: `az webapp restart --name ngr-api-prod --resource-group rg-ngr-prod`
3. If restart fails, redeploy last known-good image
4. Check Azure Status Page for platform-level incidents

### Failure 2: Database Connection Failures
**Symptoms:** API returns 500, AppInsights shows SQL connection exceptions
**Diagnosis:**
```bash
# Check SQL firewall — ensure App Service outbound IPs are whitelisted
az sql server firewall-rule list --server ngr-sql-prod --resource-group rg-ngr-prod

# Check SQL server status
az sql server show --name ngr-sql-prod --resource-group rg-ngr-prod --query state
```
**Resolution:**
1. Verify connection string in Key Vault / App Service config
2. Check SQL DTU — scale up if maxed out
3. Check private endpoint health in Azure Portal
4. Restart API to clear connection pool

### Failure 3: Okta Authentication Failures (401 on all requests)
**Symptoms:** All authenticated requests return 401, even valid tokens
**Diagnosis:**
- Check Okta System Log for token validation errors
- Verify `Okta__Authority` and `Okta__Audience` in App Service config match Okta dashboard
- Check if Okta authorization server signing keys were rotated
**Resolution:**
1. Restart API to reload OIDC discovery document and signing keys
2. If key rotation caused it: `az webapp restart --name ngr-api-prod`
3. Verify `ClockSkew` in JWT validation parameters is ≥ 2 minutes

### Failure 4: Azure Front Door 5xx / Origin Health Probe Failure
**Symptoms:** Front Door returning errors, CDN traffic not reaching App Services
**Diagnosis:**
```bash
az afd origin show \
  --resource-group rg-ngr-prod \
  --profile-name ngr-frontdoor-prod \
  --origin-group-name ngr-origins \
  --origin-name ngr-api-origin
```
**Resolution:**
1. Verify origin App Service is healthy (hit `*.azurewebsites.net` directly)
2. Check WAF rules — a rule might be blocking legitimate traffic
3. Disable WAF in Detection mode temporarily for diagnosis

### Failure 5: Deployment Failure — Container Won't Start
**Symptoms:** New deployment causes App Service to restart loop, previous version not restored
**Diagnosis:**
```bash
az webapp log tail --name ngr-api-prod --resource-group rg-ngr-prod
az webapp deployment container show-cd-url --name ngr-api-prod --resource-group rg-ngr-prod
```
**Resolution:**
1. Immediately roll back via slot swap (see Section 2)
2. Pull image locally and test: `docker run --rm ngrregistry.azurecr.io/ngr-api:<BUILD_ID>`
3. Common causes: missing env var, startup exception, health check timeout too short
4. Fix issue, rebuild image with new tag, re-deploy
