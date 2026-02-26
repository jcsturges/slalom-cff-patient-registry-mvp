resource "azurerm_linux_web_app" "main" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = var.service_plan_id
  https_only          = true

  app_settings = merge(
    var.app_settings,
    {
      "APPINSIGHTS_INSTRUMENTATIONKEY"             = var.app_insights_instrumentation_key
      "APPLICATIONINSIGHTS_CONNECTION_STRING"      = var.app_insights_connection_string
      "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    }
  )

  dynamic "connection_string" {
    for_each = var.connection_strings
    content {
      name  = connection_string.key
      type  = connection_string.value.type
      value = connection_string.value.value
    }
  }

  site_config {
    always_on                         = lookup(var.site_config, "always_on", true)
    http2_enabled                     = lookup(var.site_config, "http2_enabled", true)
    minimum_tls_version               = lookup(var.site_config, "minimum_tls_version", "1.2")
    ftps_state                        = lookup(var.site_config, "ftps_state", "Disabled")
    health_check_path                 = lookup(var.site_config, "health_check_path", null)
    health_check_eviction_time_in_min = lookup(var.site_config, "health_check_eviction_time_in_min", null)

    dynamic "application_stack" {
      for_each = lookup(var.site_config, "application_stack", null) != null ? [var.site_config.application_stack] : []
      content {
        dotnet_version = lookup(application_stack.value, "dotnet_version", null)
        node_version   = lookup(application_stack.value, "node_version", null)
      }
    }
  }

  identity {
    type = var.identity_type
  }

  logs {
    application_logs {
      file_system_level = "Information"
    }

    http_logs {
      file_system {
        retention_in_days = 7
        retention_in_mb   = 35
      }
    }
  }

  tags = var.tags
}

# VNet Integration
resource "azurerm_app_service_virtual_network_swift_connection" "main" {
  count          = var.subnet_id != null ? 1 : 0
  app_service_id = azurerm_linux_web_app.main.id
  subnet_id      = var.subnet_id
}

# Deployment slot for blue-green deployments
resource "azurerm_linux_web_app_slot" "staging" {
  name           = "staging"
  app_service_id = azurerm_linux_web_app.main.id
  https_only     = true

  app_settings = azurerm_linux_web_app.main.app_settings

  site_config {
    always_on                         = lookup(var.site_config, "always_on", true)
    http2_enabled                     = lookup(var.site_config, "http2_enabled", true)
    minimum_tls_version               = lookup(var.site_config, "minimum_tls_version", "1.2")
    ftps_state                        = lookup(var.site_config, "ftps_state", "Disabled")

    dynamic "application_stack" {
      for_each = lookup(var.site_config, "application_stack", null) != null ? [var.site_config.application_stack] : []
      content {
        dotnet_version = lookup(application_stack.value, "dotnet_version", null)
        node_version   = lookup(application_stack.value, "node_version", null)
      }
    }
  }

  identity {
    type = var.identity_type
  }

  tags = var.tags
}
