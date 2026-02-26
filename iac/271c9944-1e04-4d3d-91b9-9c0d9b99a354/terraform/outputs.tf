output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "api_app_service_name" {
  description = "Name of the API App Service"
  value       = module.api_app_service.name
}

output "api_app_service_url" {
  description = "URL of the API App Service"
  value       = "https://${module.api_app_service.default_hostname}"
}

output "web_app_service_name" {
  description = "Name of the Web App Service"
  value       = module.web_app_service.name
}

output "web_app_service_url" {
  description = "URL of the Web App Service"
  value       = "https://${module.web_app_service.default_hostname}"
}

output "sql_server_name" {
  description = "Name of the SQL Server"
  value       = module.sql.server_name
}

output "sql_database_name" {
  description = "Name of the SQL Database"
  value       = module.sql.database_name
}

output "key_vault_name" {
  description = "Name of the Key Vault"
  value       = module.keyvault.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = module.keyvault.vault_uri
}

output "application_insights_name" {
  description = "Name of Application Insights"
  value       = module.appinsights.name
}

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = module.appinsights.instrumentation_key
  sensitive   = true
}

output "storage_account_name" {
  description = "Name of the Storage Account"
  value       = azurerm_storage_account.main.name
}

output "acr_login_server" {
  description = "Login server for Azure Container Registry"
  value       = module.acr.login_server
}

output "frontdoor_endpoint" {
  description = "Front Door endpoint URL"
  value       = var.enable_waf ? module.frontdoor[0].endpoint : null
}
