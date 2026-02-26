variable "name" {
  description = "Name of the App Service"
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

variable "service_plan_id" {
  description = "App Service Plan ID"
  type        = string
}

variable "app_settings" {
  description = "App settings"
  type        = map(string)
  default     = {}
}

variable "connection_strings" {
  description = "Connection strings"
  type = map(object({
    type  = string
    value = string
  }))
  default = {}
}

variable "site_config" {
  description = "Site configuration"
  type        = any
  default     = {}
}

variable "identity_type" {
  description = "Managed identity type"
  type        = string
  default     = "SystemAssigned"
}

variable "subnet_id" {
  description = "Subnet ID for VNet integration"
  type        = string
  default     = null
}

variable "app_insights_connection_string" {
  description = "Application Insights connection string"
  type        = string
}

variable "app_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  type        = string
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
