output "id" {
  description = "Front Door ID"
  value       = azurerm_cdn_frontdoor_profile.main.id
}

output "endpoint" {
  description = "Front Door endpoint"
  value       = azurerm_cdn_frontdoor_endpoint.main.host_name
}

output "waf_policy_id" {
  description = "WAF policy ID"
  value       = azurerm_cdn_frontdoor_firewall_policy.main.id
}
