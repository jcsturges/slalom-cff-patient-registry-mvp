output "id" {
  description = "App Service ID"
  value       = azurerm_linux_web_app.main.id
}

output "name" {
  description = "App Service name"
  value       = azurerm_linux_web_app.main.name
}

output "default_hostname" {
  description = "Default hostname"
  value       = azurerm_linux_web_app.main.default_hostname
}

output "identity_principal_id" {
  description = "Managed identity principal ID"
  value       = azurerm_linux_web_app.main.identity[0].principal_id
}

output "outbound_ip_addresses" {
  description = "Outbound IP addresses"
  value       = azurerm_linux_web_app.main.outbound_ip_addresses
}
