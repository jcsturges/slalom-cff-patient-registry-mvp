environment             = "prod"
location                = "eastus"
project_name            = "ngr"
cost_center             = "CF-IT"

# App Service Plan
app_service_plan_sku    = "P2v3"

# SQL Database
sql_sku_name            = "GP_Gen5_8"
sql_max_size_gb         = 1024
sql_admin_username      = "sqladmin"
# sql_admin_password should be set via environment variable or Azure DevOps variable

# Okta Configuration
okta_domain             = "cff.okta.com"
# okta_client_id and okta_client_secret should be set via environment variable

# Networking
allowed_ip_addresses    = []
enable_private_endpoint = true

# WAF
enable_waf              = true

# Scaling
min_instances           = 2
max_instances           = 10

# Tags
tags = {
  Owner       = "Platform Team"
  Purpose     = "Production"
  Criticality = "Critical"
  Compliance  = "HIPAA"
}
