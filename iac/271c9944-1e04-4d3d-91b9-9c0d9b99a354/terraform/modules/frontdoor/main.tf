resource "azurerm_cdn_frontdoor_profile" "main" {
  name                = var.name
  resource_group_name = var.resource_group_name
  sku_name            = "Premium_AzureFrontDoor"

  tags = var.tags
}

resource "azurerm_cdn_frontdoor_endpoint" "main" {
  name                     = "${var.name}-endpoint"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = var.tags
}

resource "azurerm_cdn_frontdoor_origin_group" "main" {
  count                    = length(var.backend_pools)
  name                     = var.backend_pools[count.index].name
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  load_balancing {
    sample_size                 = 4
    successful_samples_required = 3
  }

  health_probe {
    path                = "/health"
    request_type        = "GET"
    protocol            = "Https"
    interval_in_seconds = 100
  }
}

resource "azurerm_cdn_frontdoor_origin" "main" {
  count                         = length(var.backend_pools)
  name                          = "${var.backend_pools[count.index].name}-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.main[count.index].id
  enabled                       = true

  certificate_name_check_enabled = true
  host_name                      = var.backend_pools[count.index].backend.address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.backend_pools[count.index].backend.host_header
  priority                       = 1
  weight                         = 1000
}

# WAF Policy
resource "azurerm_cdn_frontdoor_firewall_policy" "main" {
  name                              = replace("${var.name}wafpolicy", "-", "")
  resource_group_name               = var.resource_group_name
  sku_name                          = azurerm_cdn_frontdoor_profile.main.sku_name
  enabled                           = true
  mode                              = "Prevention"
  redirect_url                      = "https://www.example.com/blocked"
  custom_block_response_status_code = 403
  custom_block_response_body        = base64encode("Access denied")

  managed_rule {
    type    = "Microsoft_DefaultRuleSet"
    version = "2.1"
    action  = "Block"
  }

  managed_rule {
    type    = "Microsoft_BotManagerRuleSet"
    version = "1.0"
    action  = "Block"
  }

  tags = var.tags
}

resource "azurerm_cdn_frontdoor_security_policy" "main" {
  name                     = "${var.name}-security-policy"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  security_policies {
    firewall {
      cdn_frontdoor_firewall_policy_id = azurerm_cdn_frontdoor_firewall_policy.main.id

      association {
        domain {
          cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_endpoint.main.id
        }
        patterns_to_match = ["/*"]
      }
    }
  }
}
