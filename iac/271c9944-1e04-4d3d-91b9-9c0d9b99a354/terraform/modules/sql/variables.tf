variable "server_name" {
  description = "SQL Server name"
  type        = string
}

variable "database_name" {
  description = "SQL Database name"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "administrator_login" {
  description = "SQL Server administrator login"
  type        = string
  sensitive   = true
}

variable "administrator_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
}

variable "sku_name" {
  description = "Database SKU name"
  type        = string
}

variable "max_size_gb" {
  description = "Maximum database size in GB"
  type        = number
}

variable "allowed_ip_addresses" {
  description = "List of allowed IP addresses"
  type        = list(string)
  default     = []
}

variable "enable_private_endpoint" {
  description = "Enable private endpoint"
  type        = bool
  default     = false
}

variable "subnet_id" {
  description = "Subnet ID for private endpoint"
  type        = string
  default     = null
}

variable "virtual_network_id" {
  description = "Virtual network ID"
  type        = string
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
