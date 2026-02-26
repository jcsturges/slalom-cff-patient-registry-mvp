environment             = "test"
location                = "eastus"
project_name            = "ngr"
cost_center             = "CF-IT"

# App Service Plan
app_service_plan_sku    = "S2"

# SQL Database
sql_sku_name            = "S3"
sql_max_size_gb         = 100
sql_admin_username      = "sqladmin"
# sql_admin_password should be set via environment variable or Azure DevOps variable

# Okta Configuration
okta_domain             = "test-cff.okta.com"
# okta_client_id and okta_client_secret should be set via environment variable

# Networking
allowed_ip_addresses    = []
enable_private_endpoint = false

# WAF
enable_waf              = false

# Scaling
min_instances           = 1
max_instances           = 3

# Tags
tags = {
  Owner       = "QA Team"
  Purpose     = "Integration Testing"
  Criticality = "Medium"
}
