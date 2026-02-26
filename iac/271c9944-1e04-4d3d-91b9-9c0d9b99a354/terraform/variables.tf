variable "environment" {
  description = "Environment name (dev, test, stage, prod)"
  type        = string
  validation {
    condition     = contains(["dev", "test", "stage", "prod"], var.environment)
    error_message = "Environment must be dev, test, stage, or prod."
  }
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "eastus"
}

variable "project_name" {
  description = "Project name for resource naming"
  type        = string
  default     = "ngr"
}

variable "cost_center" {
  description = "Cost center for billing"
  type        = string
  default     = "CF-IT"
}

variable "app_service_plan_sku" {
  description = "App Service Plan SKU"
  type        = string
}

variable "sql_sku_name" {
  description = "Azure SQL Database SKU"
  type        = string
}

variable "sql_max_size_gb" {
  description = "Maximum size of SQL Database in GB"
  type        = number
}

variable "sql_admin_username" {
  description = "SQL Server administrator username"
  type        = string
  default     = "sqladmin"
  sensitive   = true
}

variable "sql_admin_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
}

variable "okta_domain" {
  description = "Okta domain"
  type        = string
}

variable "okta_client_id" {
  description = "Okta client ID"
  type        = string
  sensitive   = true
}

variable "okta_client_secret" {
  description = "Okta client secret"
  type        = string
  sensitive   = true
}

variable "allowed_ip_addresses" {
  description = "List of allowed IP addresses for SQL firewall"
  type        = list(string)
  default     = []
}

variable "enable_private_endpoint" {
  description = "Enable private endpoint for SQL Database"
  type        = bool
  default     = false
}

variable "enable_waf" {
  description = "Enable Web Application Firewall"
  type        = bool
  default     = false
}

variable "min_instances" {
  description = "Minimum number of App Service instances"
  type        = number
  default     = 1
}

variable "max_instances" {
  description = "Maximum number of App Service instances"
  type        = number
  default     = 3
}

variable "tags" {
  description = "Additional tags for resources"
  type        = map(string)
  default     = {}
}
