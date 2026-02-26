variable "name" {
  description = "Front Door name"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "backend_pools" {
  description = "Backend pool configurations"
  type = list(object({
    name = string
    backend = object({
      address     = string
      host_header = string
    })
  }))
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}
