output "server_id" {
  description = "SQL Server ID"
  value       = azurerm_mssql_server.main.id
}

output "server_name" {
  description = "SQL Server name"
  value       = azurerm_mssql_server.main.name
}

output "server_fqdn" {
  description = "SQL Server FQDN"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "database_id" {
  description = "SQL Database ID"
  value       = azurerm_mssql_database.main.id
}

output "database_name" {
  description = "SQL Database name"
  value       = azurerm_mssql_database.main.name
}

output "connection_string" {
  description = "SQL Database connection string"
  value       = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.administrator_login};Password=${var.administrator_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  sensitive   = true
}
