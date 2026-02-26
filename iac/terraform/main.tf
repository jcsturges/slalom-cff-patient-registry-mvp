locals {
  resource_prefix = "${var.project_name}-${var.environment}"
  common_tags = merge(
    {
      Environment = var.environment
      Project     = var.project_name
      CostCenter  = var.cost_center
      ManagedBy   = "Terraform"
    },
    var.tags
  )
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.resource_prefix}-${var.location}"
  location = var.location
  tags     = local.common_tags
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "vnet-${local.resource_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = ["10.0.0.0/16"]
  tags                = local.common_tags
}

# Subnets
resource "azurerm_subnet" "web" {
  name                 = "snet-web"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.1.0/24"]

  delegation {
    name = "app-service-delegation"
    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "api" {
  name                 = "snet-api"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.2.0/24"]

  delegation {
    name = "app-service-delegation"
    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "data" {
  name                 = "snet-data"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.3.0/24"]
  service_endpoints    = ["Microsoft.Sql"]
}

# Application Insights
module "appinsights" {
  source              = "./modules/appinsights"
  name                = "appi-${local.resource_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tags                = local.common_tags
}

# Azure Container Registry
module "acr" {
  source              = "./modules/acr"
  name                = "acr${var.project_name}${var.environment}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = var.environment == "prod" ? "Premium" : "Standard"
  tags                = local.common_tags
}

# Key Vault
module "keyvault" {
  source              = "./modules/keyvault"
  name                = "kv-${local.resource_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  tags                = local.common_tags
}

# Azure SQL Database
module "sql" {
  source                   = "./modules/sql"
  server_name              = "sql-${local.resource_prefix}"
  database_name            = "sqldb-${local.resource_prefix}"
  location                 = azurerm_resource_group.main.location
  resource_group_name      = azurerm_resource_group.main.name
  administrator_login      = var.sql_admin_username
  administrator_password   = var.sql_admin_password
  sku_name                 = var.sql_sku_name
  max_size_gb              = var.sql_max_size_gb
  allowed_ip_addresses     = var.allowed_ip_addresses
  enable_private_endpoint  = var.enable_private_endpoint
  subnet_id                = var.enable_private_endpoint ? azurerm_subnet.data.id : null
  virtual_network_id       = azurerm_virtual_network.main.id
  tags                     = local.common_tags
}

# Storage Account for Blob Storage
resource "azurerm_storage_account" "main" {
  name                     = "st${var.project_name}${var.environment}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = var.environment == "prod" ? "GRS" : "LRS"
  min_tls_version          = "TLS1_2"
  
  blob_properties {
    versioning_enabled = true
    delete_retention_policy {
      days = 30
    }
  }

  tags = local.common_tags
}

resource "azurerm_storage_container" "uploads" {
  name                  = "uploads"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "exports" {
  name                  = "exports"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dwfeed" {
  name                  = "dwfeed"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

# App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "asp-${local.resource_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = var.app_service_plan_sku
  tags                = local.common_tags
}

# API App Service
module "api_app_service" {
  source                       = "./modules/app-service"
  name                         = "app-${local.resource_prefix}-api"
  location                     = azurerm_resource_group.main.location
  resource_group_name          = azurerm_resource_group.main.name
  service_plan_id              = azurerm_service_plan.main.id
  app_insights_connection_string = module.appinsights.connection_string
  app_insights_instrumentation_key = module.appinsights.instrumentation_key
  
  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                    = var.environment == "prod" ? "Production" : title(var.environment)
    "KeyVault__VaultUri"                        = module.keyvault.vault_uri
    "ApplicationInsights__ConnectionString"     = module.appinsights.connection_string
    "Okta__Authority"                           = "https://${var.okta_domain}/oauth2/default"
    "Okta__Audience"                            = "api://ngr"
    "Cors__AllowedOrigins__0"                   = "https://app-${local.resource_prefix}-web.azurewebsites.net"
    "WEBSITE_RUN_FROM_PACKAGE"                  = "1"
  }

  connection_strings = {
    DefaultConnection = {
      type  = "SQLAzure"
      value = "@Microsoft.KeyVault(SecretUri=${module.keyvault.vault_uri}secrets/sql-connection-string/)"
    }
  }

  site_config = {
    always_on                         = var.environment != "dev"
    http2_enabled                     = true
    minimum_tls_version               = "1.2"
    ftps_state                        = "Disabled"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
    
    application_stack = {
      dotnet_version = "8.0"
    }
  }

  identity_type = "SystemAssigned"
  subnet_id     = azurerm_subnet.api.id
  tags          = local.common_tags
}

# Web App Service
module "web_app_service" {
  source                       = "./modules/app-service"
  name                         = "app-${local.resource_prefix}-web"
  location                     = azurerm_resource_group.main.location
  resource_group_name          = azurerm_resource_group.main.name
  service_plan_id              = azurerm_service_plan.main.id
  app_insights_connection_string = module.appinsights.connection_string
  app_insights_instrumentation_key = module.appinsights.instrumentation_key
  
  app_settings = {
    "VITE_API_URL"                              = "https://app-${local.resource_prefix}-api.azurewebsites.net"
    "VITE_OKTA_DOMAIN"                          = var.okta_domain
    "VITE_OKTA_CLIENT_ID"                       = var.okta_client_id
    "VITE_OKTA_REDIRECT_URI"                    = "https://app-${local.resource_prefix}-web.azurewebsites.net/login/callback"
    "ApplicationInsights__ConnectionString"     = module.appinsights.connection_string
    "WEBSITE_RUN_FROM_PACKAGE"                  = "1"
  }

  site_config = {
    always_on                         = var.environment != "dev"
    http2_enabled                     = true
    minimum_tls_version               = "1.2"
    ftps_state                        = "Disabled"
    
    application_stack = {
      node_version = "20-lts"
    }
  }

  identity_type = "SystemAssigned"
  subnet_id     = azurerm_subnet.web.id
  tags          = local.common_tags
}

# Auto-scaling rules for App Service Plan
resource "azurerm_monitor_autoscale_setting" "main" {
  count               = var.environment == "prod" || var.environment == "stage" ? 1 : 0
  name                = "autoscale-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  target_resource_id  = azurerm_service_plan.main.id

  profile {
    name = "default"

    capacity {
      default = var.min_instances
      minimum = var.min_instances
      maximum = var.max_instances
    }

    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.main.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 70
      }

      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT5M"
      }
    }

    rule {
      metric_trigger {
        metric_name        = "CpuPercentage"
        metric_resource_id = azurerm_service_plan.main.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "LessThan"
        threshold          = 30
      }

      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT10M"
      }
    }

    rule {
      metric_trigger {
        metric_name        = "MemoryPercentage"
        metric_resource_id = azurerm_service_plan.main.id
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT5M"
        time_aggregation   = "Average"
        operator           = "GreaterThan"
        threshold          = 80
      }

      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT5M"
      }
    }
  }

  tags = local.common_tags
}

# Azure Front Door with WAF (Production only)
module "frontdoor" {
  count               = var.enable_waf ? 1 : 0
  source              = "./modules/frontdoor"
  name                = "fd-${local.resource_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  
  backend_pools = [
    {
      name = "web-backend"
      backend = {
        address     = module.web_app_service.default_hostname
        host_header = module.web_app_service.default_hostname
      }
    },
    {
      name = "api-backend"
      backend = {
        address     = module.api_app_service.default_hostname
        host_header = module.api_app_service.default_hostname
      }
    }
  ]

  tags = local.common_tags
}

# Key Vault Access Policies
resource "azurerm_key_vault_access_policy" "api_app" {
  key_vault_id = module.keyvault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = module.api_app_service.identity_principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

resource "azurerm_key_vault_access_policy" "web_app" {
  key_vault_id = module.keyvault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = module.web_app_service.identity_principal_id

  secret_permissions = [
    "Get",
    "List"
  ]
}

# Store secrets in Key Vault
resource "azurerm_key_vault_secret" "sql_connection_string" {
  name         = "sql-connection-string"
  value        = module.sql.connection_string
  key_vault_id = module.keyvault.id

  depends_on = [
    azurerm_key_vault_access_policy.api_app
  ]
}

resource "azurerm_key_vault_secret" "okta_client_secret" {
  name         = "okta-client-secret"
  value        = var.okta_client_secret
  key_vault_id = module.keyvault.id

  depends_on = [
    azurerm_key_vault_access_policy.api_app
  ]
}

resource "azurerm_key_vault_secret" "storage_connection_string" {
  name         = "storage-connection-string"
  value        = azurerm_storage_account.main.primary_connection_string
  key_vault_id = module.keyvault.id

  depends_on = [
    azurerm_key_vault_access_policy.api_app
  ]
}

# Data source for current Azure client config
data "azurerm_client_config" "current" {}
