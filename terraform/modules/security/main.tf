# Azure Key Vault Module
# This module creates Key Vault for secure credential storage

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

# Get current client for Key Vault access policies
data "azurerm_client_config" "current" {}

# Key Vault
resource "azurerm_key_vault" "main" {
  name                       = var.key_vault_name
  location                   = var.location
  resource_group_name        = var.resource_group_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = var.key_vault_sku
  soft_delete_retention_days = var.soft_delete_retention_days
  purge_protection_enabled   = var.purge_protection_enabled

  # Network rules
  network_acls {
    bypass                     = "AzureServices"
    default_action             = var.key_vault_network_default_action
    ip_rules                   = var.key_vault_allowed_ips
    virtual_network_subnet_ids = var.key_vault_allowed_subnets
  }

  # Enable RBAC authorization (recommended over access policies)
  enable_rbac_authorization = true

  tags = merge(
    var.common_tags,
    {
      Component = "Security"
      Service   = "KeyVault"
    }
  )
}

# Store Application Insights Connection String
resource "azurerm_key_vault_secret" "application_insights_connection_string" {
  name         = "ApplicationInsights--ConnectionString"
  value        = var.application_insights_connection_string
  key_vault_id = azurerm_key_vault.main.id

  content_type = "text/plain"

  tags = merge(
    var.common_tags,
    {
      Purpose = "ApplicationInsights"
    }
  )

  depends_on = [azurerm_key_vault.main]
}

# Store Calculation Service API Key (if needed)
resource "azurerm_key_vault_secret" "calculation_service_api_key" {
  count = var.calculation_service_api_key != null ? 1 : 0

  name         = "CalculationService--ApiKey"
  value        = var.calculation_service_api_key
  key_vault_id = azurerm_key_vault.main.id

  content_type = "text/plain"

  tags = merge(
    var.common_tags,
    {
      Purpose = "ServiceAuthentication"
    }
  )

  depends_on = [azurerm_key_vault.main]
}

# Diagnostic settings for Key Vault audit logs
resource "azurerm_monitor_diagnostic_setting" "key_vault" {
  count = var.log_analytics_workspace_id != null ? 1 : 0

  name                       = "${var.key_vault_name}-diagnostics"
  target_resource_id         = azurerm_key_vault.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  # Audit logs
  enabled_log {
    category = "AuditEvent"
  }

  # Metrics
  metric {
    category = "AllMetrics"
    enabled  = true
  }
}
