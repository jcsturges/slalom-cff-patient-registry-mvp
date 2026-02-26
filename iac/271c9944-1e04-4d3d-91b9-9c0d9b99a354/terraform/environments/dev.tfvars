environment             = "dev"
location                = "eastus"
project_name            = "ngr"
cost_center             = "CF-IT"

# App Service Plan
app_service_plan_sku    = "B2"

# SQL Database
sql_sku_name            = "S2"
sql_max_size_gb         = 50
sql_admin_username      = "sqladmin"
# sql_admin_password should be set via environment variable or Azure DevOps variable

# Okta Configuration
okta_domain             = "dev-cff.okta.com"
# okta_client_id and okta_client_secret should be set via environment variable

# Networking
allowed_ip_addresses    = []
enable_private_endpoint = false

# WAF
enable_waf              = false

# Scaling
min_instances           = 1
max_instances           = 2

# Tags
tags = {
  Owner       = "DevOps Team"
  Purpose     = "Development"
  Criticality = "Low"
}
