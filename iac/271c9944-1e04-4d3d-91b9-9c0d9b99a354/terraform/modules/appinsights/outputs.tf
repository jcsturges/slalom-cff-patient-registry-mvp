output "id" {
  description = "Application Insights ID"
  value       = azurerm_application_insights.main.id
}

output "name" {
  description = "Application Insights name"
  value       = azurerm_application_insights.main.name
}

output "instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}

output "connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "app_id" {
  description = "Application Insights app ID"
  value       = azurerm_application_insights.main.app_id
}
